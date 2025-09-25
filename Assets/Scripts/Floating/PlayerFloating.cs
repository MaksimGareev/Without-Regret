using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFloating : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatForce = 10f;
    [SerializeField] private float floatDuration = 5f; // Max Amount of time the player can float for
    [SerializeField] private float floatLift = 50f; // initial height that the player lifts to when starting to float
    [SerializeField] private float horizontalSpeed = 5f; 
    [SerializeField] private float floatCooldown = 3f; // Time to cooldown between floating attempts

    [Header("Rhythm Settings")]
    [SerializeField] private float rhythmWindow = 0.3f; // Margin of error so the input doesn't have to be exactly perfectly timed
    [SerializeField] private float rhythmInterval = 1f; // Time in between each input
    [SerializeField] private Slider rhythmSlider; // UI element to update and show floating

    [Header("Input")]
    [SerializeField] private KeyCode floatKey = KeyCode.Space;

    private Rigidbody rb;
    private bool isFloating = false;
    private float floatTimer = 0f;
    private float rhythmTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isCoolingDown = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        HandleCooldown();

        if (!isCoolingDown)
        {
            if (!isFloating && Input.GetKeyDown(floatKey))
            {
                StartFloating();
            }

            if (isFloating)
            {
                HandleRhythmInput();
                UpdateRhythmUI();
            }
        }  
    }

    private void FixedUpdate()
    {
        if (isFloating)
        {
            ApplyFloatPhysics();
        }
    }

    private void HandleRhythmInput()
    {
        rhythmTimer += Time.deltaTime;
        floatTimer += Time.deltaTime;

        if (floatTimer >= floatDuration)
        {
            StopFloating();
            return;
        }

        if (floatTimer >= rhythmInterval)
        {
            StopFloating();
            floatTimer = 0f;
            if (rhythmSlider != null)
            {
                rhythmSlider.value = 0f;
            }
        }

        if (Input.GetKeyDown(floatKey))
        {
            float distanceToRhythm = Mathf.Min(floatTimer, rhythmInterval - rhythmTimer);

            if (distanceToRhythm <= rhythmWindow)
            {
                floatTimer = 0f;

                if (rhythmSlider != null)
                {
                    rhythmSlider.value = 0f;
                }

                Debug.Log("Floating Rhythm Success");
            }
            else
            {
                Debug.Log("Floating failed");
                StopFloating();
            }
        }
    }

    private void UpdateRhythmUI()
    {
        if (rhythmSlider != null)
        {
            float normalized = rhythmTimer / rhythmInterval;
            rhythmSlider.value = normalized;
        }
    }

    private void ApplyFloatPhysics()
    {

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontalInput, 0f, verticalInput) * horizontalSpeed;
        move = transform.TransformDirection(move);

        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
    }

    private void StartFloating()
    {
        isFloating = true;
        floatTimer = 0f;
        rhythmTimer = 0f;

        rb.AddForce(Vector3.up * floatLift, ForceMode.VelocityChange);
    }

    private void StopFloating()
    {
        isFloating = false;
        floatTimer = 0f;

        if (rhythmSlider != null)
        {
            rhythmSlider.value = 0f;
        }

        isCoolingDown = true;
        cooldownTimer = floatCooldown;
    }

    private void HandleCooldown()
    {
        if (isCoolingDown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isCoolingDown = false;
            }
        }
    }
}
