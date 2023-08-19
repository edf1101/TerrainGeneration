using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{

    private Rigidbody rb;
    [SerializeField] private float LerpSpeed;
    [SerializeField] private float motionSpeed;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float cursorSpeed;

    private float rotX, rotY;

    private bool mouseLocked;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mouseLocked = true;
    }

    private Vector3 aimMovementVector;
    private Vector3 currentVelocity;
    // Update is called once per frame
    void Update()
    {

        if (mouseLocked)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;


        if (mouseLocked)
        {
            motionSpeed += Input.GetAxis("Mouse ScrollWheel") * cursorSpeed;
            motionSpeed = Mathf.Clamp(motionSpeed, 2, 70);


            Vector3 newF = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 newR = new Vector3(transform.right.x, 0, transform.right.z);
            Vector3 aimMovementVector = (newF * Input.GetAxis("Vertical") + newR * Input.GetAxis("Horizontal") + Vector3.up * Input.GetAxis("Depth")).normalized * 1 * motionSpeed;

            currentVelocity = Vector3.Lerp(currentVelocity, aimMovementVector, Time.deltaTime * LerpSpeed);

            rb.velocity = currentVelocity;


            rotX += Input.GetAxis("Mouse X") * lookSpeed;
            rotY -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotY = Mathf.Clamp(rotY, -90, 90);

            transform.localRotation = Quaternion.Euler(rotY, rotX, 0);

        }
    }
}
