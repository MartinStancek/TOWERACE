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
    public float animChickenMaxSpeed = 2f;

    public SkinnedMeshRenderer chickenSkin;
    public GameObject carSkin;
    public ParticleSystem chickenEffect;

    public float chickenBoost = 1.2f;
    private float actualChickenBoost = 1f;

    private float wheelRotationInput = 0;
    public float magnitudePowMultiplier = 2f;

    public Player player;
    private float maxSpeedvelocity = 100f;
    // Start is called before the first frame update

    public AudioSource engineSound;

    void Start()
    {
        rb.transform.parent = null;
        originalAccel = forwardAccel;
        /*GameController.Instance.onStartRace.AddListener(() =>
        {
            verticalInput = player.playerInput.actions.actionMaps[0].v
        })*/
    }

    public void OnAcceleration(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            var input = context.ReadValue<float>();

            //Debug.Log("OnAcceleration: " + context);

            OnAcceleration(input);
        }
    }

    public void OnAcceleration(float value)
    {
        verticalInput = value;
    }

    public void OnSteering(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<float>();
        //Debug.Log("OnMove: " + input);

        OnSteering(input);
    }

    public void OnSteering(float value)
    {
        horizontalInput = value;

    }

    void Update()
    {
        var accel = 1f;
        if (verticalInput > 0)
        {
            accel = forwardAccel;
        }
        else if (verticalInput < 0)
        {
            accel = reverseAccel;
        }
        speedInput = Mathf.Lerp(speedInput, verticalInput * accel * 1000f, Time.deltaTime * speedGrainMultiplier);
        direction = speedInput > 0 ? 1 : -1;


        player.outline.SetSpeedBar(rb.velocity.magnitude / maxSpeedvelocity);
        engineSound.pitch = (grounded) ? (rb.velocity.magnitude / maxSpeedvelocity) * 2 + 1 : 2;

        //Debug.Log(rb.velocity.magnitude * animChickenMultiplier);
        wheelRotationInput = Mathf.Lerp(wheelRotationInput, horizontalInput, Time.deltaTime * turnSpeed);

        var clampedMagnitude = Mathf.Clamp(Mathf.Pow(rb.velocity.magnitude, magnitudePowMultiplier), -maxCarRotationDelta, maxCarRotationDelta);
        turnInput = Mathf.Lerp(turnInput, horizontalInput * turnStrength * clampedMagnitude * direction, Time.deltaTime * turnSpeed);
        if (grounded)
        {
            if (animChicken.isActiveAndEnabled)
            {
                var speed = Mathf.Clamp(rb.velocity.magnitude * animChickenMultiplier, -animChickenMaxSpeed, animChickenMaxSpeed);
                animChicken.SetFloat("Speed", speed);
            }

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * Time.deltaTime, 0f));
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
        leftFrontWheel.localEulerAngles = new Vector3(leftFrontWheel.localEulerAngles.x, (wheelRotationInput * maxWheelTurn)/* - 180*/, 0f);
        rightFrontWheel.localEulerAngles = new Vector3(rightFrontWheel.localEulerAngles.x, wheelRotationInput * maxWheelTurn, 0f);




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
                rb.AddForce(transform.forward * speedInput * actualChickenBoost, ForceMode.Force);

                emissionRate = maxEmission;
            }
        }
        else
        {
            rb.drag = 0.1f;
            rb.AddForce(Vector3.up * -gravityForce * 100f, ForceMode.Force);
        }
    }

    public void RestartPostion(Vector3 targetPosition, Quaternion targetRotation)
    {
        RestartPostion(targetPosition, targetRotation, 0f);
    }

    public void RestartPostion(Vector3 targetPosition, Quaternion targetRotation, float slowFactor)
    {
        rb.MovePosition(targetPosition);
        transform.rotation = targetRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        speedInput *= slowFactor;
        turnInput *= slowFactor;
    }

    public void SetChickenSkin()
    {
        actualChickenBoost = chickenBoost;
        if (!chickenSkin.gameObject.activeInHierarchy)
        {
            chickenSkin.gameObject.SetActive(true);
            chickenEffect.Play();
            carSkin.SetActive(false);
            player.outline.wingItPanel.gameObject.SetActive(true);
            SoundManager.PlaySound(SoundManager.SoundType.CHICKEN_TRANSMISION);
        }
    }

    public void SetCarSkin()
    {
        actualChickenBoost = 1f;
        if (!carSkin.activeInHierarchy)
        {
            chickenSkin.gameObject.SetActive(false);
            carSkin.SetActive(true);
            chickenEffect.Play();
            player.outline.wingItPanel.gameObject.SetActive(false);
            SoundManager.PlaySound(SoundManager.SoundType.CAR_TRANSMISION);

        }
    }



}
