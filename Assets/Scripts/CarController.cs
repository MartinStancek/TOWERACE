using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public Rigidbody rb;

    [HideInInspector]
    public float originalAccel;
    public float forwardAccel = 8f;
    public float reverseAccel = 4f;
    public float maxSpeed = 50f;
    public float turnStrength = 180f;
    public float gravityForce = 10f;
    public float dragOnGround = 3f;
    public float dragOnFly = 0.1f;

    private bool grounded;
    public float speedGrainMultiplier = 3f;
    public LayerMask whatIsGround;
    public float groundRayLength = 0.5f;
    public Transform groundRayPoint;

    public Transform leftFrontWheel;
    public Transform rightFrontWheel;
    public float maxWheelTurn = 25f;
    public float maxCarRotationDelta = 1f;

    public ParticleSystem[] dustTrial;
    public float maxEmission = 25f;
    private float emissionRate;

    private float speedInput, turnInput;

    private float verticalInput;
    private float horizontalInput;

    public List<Transform> wheels;
    public float wheelRotationSpeed = 0.1f;

    public bool isActivated = true;
    public float turnSpeed = 0.1f;
    private int direction = 1;

    public Animator animChicken;
    public float animChickenMultiplier = 10f;

    public GameObject chickenSkin;
    public GameObject carSkin;

    public float chickenBoost = 1.2f;
    private float actualChickenBoost = 1f;


    // Start is called before the first frame update
    void Start()
    {
        rb.transform.parent = null;
        originalAccel = forwardAccel;
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
        var accel = 1f;
        if (verticalInput > 0)
        {
            accel = forwardAccel;
            direction = 1;
        }
        else if (verticalInput < 0)
        {
            accel = reverseAccel;
            direction = -1;
        }
        speedInput = Mathf.Lerp(speedInput, verticalInput * accel * 1000f, Time.deltaTime * speedGrainMultiplier);
        //Debug.Log(rb.velocity.magnitude * animChickenMultiplier);
        turnInput = Mathf.Lerp(turnInput, horizontalInput, Time.deltaTime * turnSpeed);

        if (grounded)
        {
            if (animChicken.isActiveAndEnabled) animChicken.SetFloat("Speed", rb.velocity.magnitude * animChickenMultiplier);
            var rotationDelta = Mathf.Clamp(turnInput * turnStrength * Time.deltaTime * speedInput, -maxCarRotationDelta, maxCarRotationDelta);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, rotationDelta, 0f));
            //Debug.Log("rotationDelta: " + rotationDelta);
        }
        else if (animChicken.isActiveAndEnabled)
        {
            animChicken.SetFloat("Speed", 0f);
        }
        foreach (var t in wheels)
        {
            t.Rotate(Vector3.right, wheelRotationSpeed * Time.deltaTime * rb.velocity.magnitude * direction, Space.Self);
        }
        leftFrontWheel.localEulerAngles = new Vector3(leftFrontWheel.localEulerAngles.x, (turnInput * maxWheelTurn)/* - 180*/, 0f);
        rightFrontWheel.localEulerAngles = new Vector3(rightFrontWheel.localEulerAngles.x, turnInput * maxWheelTurn, 0f);




        transform.position = rb.transform.position;

        //Debug.Log(rb.velocity.magnitude);
    }

    void FixedUpdate()
    {
        grounded = false;
        RaycastHit hit;

        if (!isActivated)
        {
            return;
        }

        Debug.DrawRay(groundRayPoint.position, -transform.up, Color.green, groundRayLength);

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround) && isActivated)
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
                rb.AddForce(transform.forward * speedInput * actualChickenBoost);

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
    public void RestartPostion(Vector3 targetPosition)
    {
        rb.MovePosition(targetPosition);
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void SetChickenSkin()
    {
        chickenSkin.SetActive(true);
        carSkin.SetActive(false);
        actualChickenBoost = chickenBoost;
    }

    public void SetCarSkin()
    {
        chickenSkin.SetActive(false);
        carSkin.SetActive(true);
        actualChickenBoost = 1f;
    }



}
