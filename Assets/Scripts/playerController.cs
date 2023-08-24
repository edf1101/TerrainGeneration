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
    [SerializeField] private bool flying; // whether fly  mode or walk mode

    private Vector3 rotation; // values for camera rotation

    private bool mouseLocked; // is the mouse locked

    private bool torchOut; // whether torch in use or not

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>(); // set the rigidbody variable
        mouseLocked = true; // mouse locked initially
        myAnimator = GetComponentInChildren<Animator>();
        footStepAudioSource = GetComponent<AudioSource>();

        motionSpeed = 5; // speed for walking
        
    }

    // store our current velocity so we dont have to keep getting it from rigidbody
    private Vector3 currentVelocity;
    private Vector3 aimMovementVector;

    private Vector2 currentTile; // current tile location

    private float lastCheckedBiome; // used for updating biome location each n seconds
    private biomeDescription currentBiome; // holds current biomes

    private Animator myAnimator; // reference to the animator for the player
    private bool footStepReady; // this bool helps the footstep only get registered once per animation loop

    private AudioSource footStepAudioSource; // reference to the audio source for footsteps
    private bool moving; // whether player moving or not 

    private float lastJump; // used to detect double jumps

    private float lastEsc; // used to detect double escape

    void updateAnimations() // this used to sort all animation stuff
    {
        float animatorTime = myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime%myAnimator.GetCurrentAnimatorStateInfo(0).length  ;
        myAnimator.SetBool("moving", moving);
        if (animatorTime > 0.9f && footStepReady)   
        {
            footStepReady = false;
        }
        
        if (animatorTime<0.1f && !footStepReady)
        {
            footStepReady = true;
            // code for on footstep here
            if (currentBiome != null && moving && !flying)
            {
                // checks we can actually look at biome else we may have errors
                footStepAudioSource.Play();
                  
            }
          

        }
        
    }


    // Update is called once per frame
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastJump < 0.8f)
            { // been a double jump
                flying = !flying;
            }

            lastJump = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.time - lastEsc < 0.8f)
            { // been a double escape
                Application.Quit();
            }

            lastEsc = Time.time;
        }

        updateAnimations();

        if (Time.time - lastCheckedBiome > 2    ) // if its been 2s since last checked
        {
            lastCheckedBiome = Time.time;

            // calculate current tile
            currentTile = new Vector2(Mathf.FloorToInt(transform.position.x / 100), Mathf.FloorToInt(transform.position.z / 100));

            // calculate where I am within that tile
            Vector2 smallPart = new Vector2(transform.position.x, transform.position.z) - currentTile * 100;
            smallPart = new Vector2((int)smallPart.x, (int)smallPart.y);

            // fetch current biome from that data
            currentBiome= TerrainManager.getTileObject(currentTile).GetComponent<tileManager>().getBiomeAt(smallPart);
            footStepAudioSource.clip = currentBiome.footStepSound;

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
            if (flying)
            {
                motionSpeed += Input.GetAxis("Mouse ScrollWheel") * cursorSpeed;
                motionSpeed = Mathf.Clamp(motionSpeed, 2, 70); // clamp the speed 

            }
            else
            {
                motionSpeed = 5.5f;
            }


            /*  create new vectors for transform.forward / transform.right
             *  we want to remove the vertical (y) direction component from them
             *  So it moves in a minecraft creative mode kind of feel
             */ 
            Vector3 newF = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 newR = new Vector3(transform.right.x, 0, transform.right.z);

            // We then move in the new forward direction * forward input then
            // in right direction * right input then add Vector3.up * depth (up/down)
            aimMovementVector = (newF * Input.GetAxis("Vertical") + newR * Input.GetAxis("Horizontal") + Vector3.up * (flying?1:0)*Input.GetAxis("Depth")).normalized * 1 * motionSpeed;

            // current veclocity should be an interpolation between current veclocity
            // and aim velocity this should make it a smooth velocity
            currentVelocity = (!flying?aimMovementVector:Vector3.Lerp(currentVelocity, aimMovementVector, Time.deltaTime * LerpSpeed));
            if(!flying)
            currentVelocity.y = rb.velocity.y-9.81f*Time.deltaTime*2;
            // set the new current velocity to the rigidbody velocity so it actually moves
            rb.velocity = currentVelocity;


            // mouse can rotate in x and Y direction
            rotation = new Vector3(rotation.x + Input.GetAxis("Mouse X") * lookSpeed, Mathf.Clamp(rotation.y - Input.GetAxis("Mouse Y") * lookSpeed,-90,90), 0);
           
           

            // set the rotation

            // the player should only move around y axis as it shouldnt tilt up/down
            transform.localEulerAngles = new Vector3(0, rotation.x, 0);
            // the camera should tilt up/ down though
            myCamera.transform.localRotation = Quaternion.Euler(rotation.y, 0, 0);

            moving = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);

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
