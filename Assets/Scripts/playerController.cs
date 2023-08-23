using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * By www.github.com/edf1101
 * Simple Player controller script that can just fly around a scene nicely
 * Required components: Rigidbody (recommended: A collider)
 * Reccommended settings: 
 *      rigidbody contstraints: freeze all angles
 *      LerpSpeed = 2
 *      motionSpeed = 15
 *      lookSpeed = 3
 *      cursorSpeed = 10
 */

public class playerController : MonoBehaviour
{
    [Header("Transform references")]
    [SerializeField] private Transform myCamera;
    [SerializeField] private Transform myTorch;
    private Rigidbody rb; // reference to the rigidbody component

    [Header("Movement parameters")]
    // parameters for the player controller
    [SerializeField] private float LerpSpeed; // how fast it reaches the speed its aiming for (gliding kinda)
    [SerializeField] private float motionSpeed; // how fast it moves m/s
    [SerializeField] private float lookSpeed; // how sensitive the mouse is
    [SerializeField] private float cursorSpeed; // scroll speed (raising motionSpeed)

    private Vector3 rotation; // values for camera rotation

    private bool mouseLocked; // is the mouse locked

    private bool torchOut; // whether torch in use or not

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>(); // set the rigidbody variable
        mouseLocked = true; // mouse locked initially
    }

    // store our current velocity so we dont have to keep getting it from rigidbody
    private Vector3 currentVelocity;
    private Vector3 aimMovementVector;

    private Vector2 currentTile; // current tile location

    private float lastCheckedBiome; // used for updating biome location each n seconds
    private biomeDescription currentBiome; // holds current biomes


    // Update is called once per frame
    private void Update()
    {

        if (Time.time - lastCheckedBiome > 2) // if its been 2s since last checked
        {
            lastCheckedBiome = Time.time;

            // calculate current tile
            currentTile = new Vector2(Mathf.FloorToInt(transform.position.x / 100), Mathf.FloorToInt(transform.position.z / 100));

            // calculate where I am within that tile
            Vector2 smallPart = new Vector2(transform.position.x, transform.position.z) - currentTile * 100;
            smallPart = new Vector2((int)smallPart.x, (int)smallPart.y);

            // fetch current biome from that data
            currentBiome= TerrainManager.getTileObject(currentTile).GetComponent<tileManager>().getBiomeAt(smallPart);
            

        }

        // setting cursor lock state
        if (mouseLocked)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;


        if (Input.GetKeyDown(KeyCode.Escape)) // if escape pressed down
        {
            if (mouseLocked) // toggle mouseLocked state
                mouseLocked = false;
            else
                mouseLocked = true;
        }


        // only move/look around if mouse is locked so user is focused
        if (mouseLocked)
        {

            // toggle torch being on or off
            if (Input.GetKeyDown(KeyCode.T))
            {
                torchOut = !torchOut;
                myTorch.gameObject.SetActive(torchOut);
            }



            // scroll wheel can change the motion speed
            motionSpeed += Input.GetAxis("Mouse ScrollWheel") * cursorSpeed;
            motionSpeed = Mathf.Clamp(motionSpeed, 2, 70); // clamp the speed 


            /*  create new vectors for transform.forward / transform.right
             *  we want to remove the vertical (y) direction component from them
             *  So it moves in a minecraft creative mode kind of feel
             */ 
            Vector3 newF = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 newR = new Vector3(transform.right.x, 0, transform.right.z);

            // We then move in the new forward direction * forward input then
            // in right direction * right input then add Vector3.up * depth (up/down)
             aimMovementVector = (newF * Input.GetAxis("Vertical") + newR * Input.GetAxis("Horizontal") + Vector3.up * Input.GetAxis("Depth")).normalized * 1 * motionSpeed;

            // current veclocity should be an interpolation between current veclocity
            // and aim velocity this should make it a smooth velocity
            currentVelocity = Vector3.Lerp(currentVelocity, aimMovementVector, Time.deltaTime * LerpSpeed);

            // set the new current velocity to the rigidbody velocity so it actually moves
            rb.velocity = currentVelocity;


            // mouse can rotate in x and Y direction
            rotation = new Vector3(rotation.x + Input.GetAxis("Mouse X") * lookSpeed, Mathf.Clamp(rotation.y - Input.GetAxis("Mouse Y") * lookSpeed,-90,90), 0);
           
           

            // set the rotation

            // the player should only move around y axis as it shouldnt tilt up/down
            transform.localEulerAngles = new Vector3(0, rotation.x, 0);
            // the camera should tilt up/ down though
            myCamera.transform.localRotation = Quaternion.Euler(rotation.y, 0, 0);

        }
        else
        {
            aimMovementVector=Vector3.zero;
        }

        currentVelocity = Vector3.Lerp(currentVelocity, aimMovementVector, Time.deltaTime * LerpSpeed);

        // set the new current velocity to the rigidbody velocity so it actually moves
        rb.velocity = currentVelocity;
    }
}
