using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public Rigidbody rb;

    public float forwardAccel = 8f;
    public float reverseAccel = 4f; 
    public float maxSpeed = 50f;
    public float turnStrength = 180f;
    public float gravityForce = 10f;
    public float dragOnGround = 3f;

    private bool grounded;

    public LayerMask whatIsGround;
    public float groundRayLength = 0.5f;
    public Transform groundRayPoint;

    public Transform leftFrontWheel;
    public Transform rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustTrial;
    public float maxEmission = 25f;
    private float emissionRate;

    private float speedInput, turnInput;

    private float verticalInput;
    private float horizontalInput;

    public bool isActivated = true;


    // Start is called before the first frame update
    void Start()
    {
        rb.transform.parent = null;
        SetEmmision(0f);
    }

    public void OnAcceleration(InputAction.CallbackContext context)
    {

        var input = context.ReadValue<float>();
        //Debug.Log("OnMove: " + input);

        verticalInput = input;
    }
    public void OnSteering(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<float>();
        //Debug.Log("OnMove: " + input);

        horizontalInput = input;
    }

    void Update()
    {
        speedInput = 0f;
        if(verticalInput > 0)
        {
            speedInput = verticalInput * forwardAccel * 1000f;
        }
        else if (verticalInput < 0)
        {
            speedInput = verticalInput * reverseAccel * 1000f;
        }

        turnInput = horizontalInput;

        if (grounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * verticalInput, 0f));
        }

        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, turnInput * maxWheelTurn, rightFrontWheel.localRotation.eulerAngles.z);
        
        transform.position = rb.transform.position;
    }

    void FixedUpdate()
    {
        if (!isActivated)
        {
            return;
        }

        grounded = false;
        RaycastHit hit;

        if(Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        emissionRate = 0f;

        if (grounded)
        {
            rb.drag = dragOnGround;

            if (Mathf.Abs(speedInput) > 0)
            {
                rb.AddForce(transform.forward * speedInput);

                emissionRate = maxEmission;
            }
        } 
        else
        {
            rb.drag = 0.1f;
            rb.AddForce(Vector3.up * -gravityForce * 100f);
        }

        SetEmmision(emissionRate);
    }

    private void SetEmmision(float emissionRate)
    {
        foreach (var p in dustTrial)
        {
            var emissionModule = p.emission;
            emissionModule.rate = emissionRate;
        }
    }

    public void RestartPostion()
    {

    }

}
