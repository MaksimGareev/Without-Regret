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
    [SerializeField] private float horizontalSpeed = 10f; // Movement speed while floating
    [SerializeField] private float floatCooldown = 3f; // Time to cooldown between floating attempts
    private float floatHeight;

    [Header("Rhythm Settings")]
    [SerializeField] private float rhythmWindow = 0.3f; // Margin of error so the input doesn't have to be exactly perfectly timed
    [SerializeField] private float rhythmInterval = 1f; // Time in between each input
    [SerializeField] private Slider rhythmSlider; // UI element to update and show floating

    [Header("Input")]
    [SerializeField] private KeyCode floatKey = KeyCode.Space; // Keyboard input
    [SerializeField] private string floatButton = "Submit"; // Controller input

    private PlayerController playerController;
    private CharacterController characterController;
    private Rigidbody playerRigidBody;
    private Camera playerCamera;

    // variables for managing the rhythm of the floating mechanic
    private bool isFloating = false;
    private float floatTimer = 0f; // Used to keep track of how long the player has been floating for
    private float rhythmTimer = 0f; // Keeps track of the time when the player hits spacebar again
    private float cooldownTimer = 0f; // How long the player has to wait after floating until they can start floating again
    private bool isCoolingDown = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        playerRigidBody = GetComponent<Rigidbody>();
        playerCamera = Camera.main;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleCooldown();

        if (!isCoolingDown)
        {
            // Start floating if the player presses the appropriate buttons
            if (!isFloating && (Input.GetKeyDown(floatKey) || Input.GetButtonDown(floatButton)))
            {
                StartFloating();
            }

            // Keep the rhythm 
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

    private void StartFloating()
    {
        isFloating = true;
        floatTimer = 0f;
        rhythmTimer = 0f;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        playerRigidBody.AddForce(Vector3.up * floatLift, ForceMode.VelocityChange);
    }

    private void StopFloating()
    {
        isFloating = false;
        rhythmTimer = 0f;
        floatTimer = 0f;

        if (rhythmSlider != null)
        {
            rhythmSlider.value = 0f;
        }

        isCoolingDown = true;
        cooldownTimer = floatCooldown;

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    private void HandleRhythmInput()
    {
        rhythmTimer += Time.deltaTime;
        floatTimer += Time.deltaTime;

        if (floatTimer >= floatDuration)
        {
            StopFloating();
            Debug.Log("Floating failed: Ran out of floating time");
            return;
        }

        if (Input.GetKeyDown(floatKey) || Input.GetButtonDown(floatButton))
        {
            float errorMargin = Mathf.Min(rhythmTimer, rhythmInterval - rhythmTimer);

            if (errorMargin <= rhythmWindow)
            {
                rhythmTimer = 0f;
                if (rhythmSlider != null)
                {
                    rhythmSlider.value = 0f;
                }
                Debug.Log("Floating Rhythm Success");
            }
            else
            {
                Debug.Log("Floating failed 3: Missed timing");
                StopFloating();
            }
        }

        if (rhythmTimer >= rhythmInterval + rhythmWindow)
        {
            Debug.Log("Floating failed: Missed timing");
            StopFloating();
        }
    }

    private void UpdateRhythmUI()
    {
        if (rhythmSlider != null)
        {
            float normalized = Mathf.Clamp01(rhythmTimer / rhythmInterval);
            rhythmSlider.value = normalized;
        }
    }

    private void ApplyFloatPhysics()
    {
        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);
        playerRigidBody.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);

        Vector3 move = CalculateInputFromPOV() * horizontalSpeed;

        playerRigidBody.MovePosition(playerRigidBody.position + move * Time.fixedDeltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
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
    
    private Vector3 CalculateInputFromPOV()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (input.sqrMagnitude < 0.1f)
        {
            return Vector3.zero;
        }

        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 relativeDirection = (camForward * input.z + camRight * input.x).normalized;
        return relativeDirection;
    }
}
