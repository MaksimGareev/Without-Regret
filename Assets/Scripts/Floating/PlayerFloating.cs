using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFloating : MonoBehaviour
{
    [Header("Float Settings")]
    // [SerializeField] private float floatForce = 10f;
    [SerializeField] private float floatDuration = 5f; // Max Amount of time the player can float for 
    [SerializeField] private float floatLift = 5f; // initial height that the player lifts to when starting to float
    // [SerializeField] private float horizontalSpeed = 10f; // Movement speed while floating
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

    // variables for managing the rhythm of the floating mechanic
    private bool isFloating = false;
    private float floatTimer = 0f; // Used to keep track of how long the player has been floating for
    private float rhythmTimer = 0f; // Keeps track of the time when the player hits spacebar again
    private float cooldownTimer = 0f; // How long the player has to wait after floating until they can start floating again
    private bool isCoolingDown = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
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
            playerController.SetGravityEnabled(false);
            floatHeight = transform.position.y + floatLift;
            playerController.SetVerticalVelocity(0f);
        }
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
            playerController.SetGravityEnabled(true);
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
        if (playerController == null)
        {
            return;
        }

        float currentY = transform.position.y;
        float heightError = floatHeight - currentY;

        // Use proportional–derivative control (spring + damping)
        float correction = heightError * 5f; // proportional factor (hover stiffness)
        float damping = -playerController.GetVerticalVelocity() * 2f; // cancel out velocity drift

        float newVerticalVelocity = correction + damping;

        // Clamp to avoid extreme forces
        newVerticalVelocity = Mathf.Clamp(newVerticalVelocity, -5f, 5f);

        playerController.SetVerticalVelocity(newVerticalVelocity);
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
