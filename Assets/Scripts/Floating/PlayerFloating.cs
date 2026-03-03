using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFloating : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField, Tooltip("How powerful the instant upward boost on floating start is")] 
    private float floatLift = 3f;
    [SerializeField, Tooltip("Height above start position to hover at")] 
    private float floatHeightOffset = 3f;
    [SerializeField, Tooltip("Movement speed while floating")] 
    private float horizontalSpeed = 6f;
    [SerializeField, Tooltip("Affects how quickly changes in Y occur")] 
    private float verticalSmooth = 5f;
    [SerializeField, Tooltip("Affects how quickly horizontal movement interpolates")] 
    private float moveSmoothing = 8f;
    [SerializeField, Tooltip("Air drag while hovering")] 
    private float hoverDrag = 3f;
    [SerializeField, Tooltip("Deadzone for controllers")] 
    private float stickDeadzone = 0.2f;

    [Header("Rhythm / Input")]
    [SerializeField, Tooltip("Control input for floating")] 
    private InputActionReference floatAction;
    [SerializeField] private float floatDuration = 5f;
    [SerializeField] private float floatCooldown = 3f;
    [SerializeField, Tooltip("Determines the size of the success window")]
    private float rhythmWindow = 0.3f;
    [SerializeField, Tooltip("Determines the size of the rhythm bar")] 
    private float rhythmInterval = 1f;
    [SerializeField, Tooltip("Determines how low the rhythmWindow can be")]
    private float rhythmWindowMin = 0.33f;
    [SerializeField, Tooltip("Defines a buffer for determining success. If the player misses the window, but the input is within the buffer, it still counts as a success")]
    private float rhythmBuffer = 0.15f;
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool floatInput;
    private bool failState = false;
    private Image cooldownFillImage;
    private Color originalCooldownColor;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private Rigidbody rb;
    private PlayerController playerController;
    private CharacterController charController;
    private Camera playerCamera;
    private ToggleInventoryUI toggleInventoryUI;
    private Animator animator;
    private RectTransform floatTargetArea;
    private Slider floatingSlider;
    private Slider cooldownSlider;

    [Header("Chime Animation settings")]
    public Animator chimeAnimator;
    public bool chimeActive = false;
    public Chime chimeScript;


    public bool IsFloating { get; private set; } = false;
    private bool canFloat = false;
    private float floatTimer = 0f;
    private float cooldownTimer = 0f;
    public bool IsCoolingDown { get; private set; } = false;
    private float rhythmTimer = 0f;
    private float hoverTargetY;

    private Vector3 currentMove = Vector3.zero;
    private Vector3 targetMove = Vector3.zero;

    private bool prevRbUseGravity;
    private float prevRbDrag;
    private bool prevRbKinematic;

    // randomized window (normalized 0..1)
    private float windowStartNormalized = 0f;
    private float windowWidthNormalized = 0f;

    void OnEnable()
    {
        controls.Enable();
        floatAction.action.performed += ctx => ReadSubmit(ctx);
        floatAction.action.canceled += ctx => ReadSubmit(ctx);
        controls.Player.Move.performed += ctx => ReadMove(ctx);
        controls.Player.Move.canceled += ctx => ReadMove(ctx);
    }
    void OnDisable()
    {
        controls.Disable();
        floatAction.action.performed -= ctx => ReadSubmit(ctx);
        floatAction.action.canceled -= ctx => ReadSubmit(ctx);
        controls.Player.Move.performed -= ctx => ReadMove(ctx);
        controls.Player.Move.canceled -= ctx => ReadMove(ctx);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        charController = GetComponent<CharacterController>();
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
        playerCamera = Camera.main;
        controls = new PlayerControls();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("PlayerFloating: Animator component could not be found. Please ensure there is an Animator component on a child object for handling floating animations.");
            }
        }

        //Finding chime + animator
        GameObject chime = GameObject.FindWithTag("Chime");
        if (chime != null)
        {
            chimeScript = chime.GetComponent<Chime>();
            chimeAnimator = chime.GetComponentInChildren<Animator>();

            chimeActive = true;
        }


        if (GameManager.Instance.floatCooldown != null)
        {
            cooldownSlider = GameManager.Instance.floatCooldown;
            cooldownFillImage = cooldownSlider.fillRect.GetComponent<Image>();
            originalCooldownColor = cooldownFillImage.color;
            cooldownSlider.gameObject.SetActive(false);
        }

        if (GameManager.Instance.floatingSlider != null)
        {
            floatingSlider = GameManager.Instance.floatingSlider;
        }

        if (GameManager.Instance.floatTargetArea != null)
        {
            floatTargetArea = GameManager.Instance.floatTargetArea;
        }

        // Assign floatInput based on the state of the Input Action
        floatAction.action.performed += ctx => ReadSubmit(ctx);
        floatAction.action.canceled += ctx => ReadSubmit(ctx);
        controls.Player.Move.performed += ctx => ReadMove(ctx);
        controls.Player.Move.canceled += ctx => ReadMove(ctx);

        if (floatAction == null)
        {
            Debug.LogError("PlayerFloating: Float Input Action Reference is not assigned.");
        }
    }

    public void ReadSubmit(InputAction.CallbackContext context)
    {
        floatInput = context.action.triggered;

        //if (showDebugLogs && canFloat)
        //{
        //    Debug.Log("Float Input: " + floatInput);
        //}
    }

    public void ReadMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        //if (showDebugLogs && isFloating)
        //{
        //    Debug.Log("PlayerFloating - Move Input: " + moveInput);
        //}
    }

    private void Start()
    {
        SetupRhythmTargets();
        if (floatingSlider != null)
            floatingSlider.gameObject.SetActive(false);
    }

    private void SetupRhythmTargets()
    {
        if (floatingSlider == null || floatTargetArea == null)
            return;

        if (rhythmInterval <= 0f)
        {
            if (showDebugLogs) Debug.LogWarning("PlayerFloating: rhythmInterval must be > 0 to position targets.");
            return;
        }

        // compute normalized window width
        float clampedWindow = Mathf.Clamp(rhythmWindow, 0f, rhythmInterval);
        windowWidthNormalized = Mathf.Clamp01(clampedWindow / rhythmInterval);

        // choose a single random start position so the window is [start, start+width]
        windowStartNormalized = Random.Range(rhythmWindowMin, 1f - windowWidthNormalized);
        float windowEndNormalized = windowStartNormalized + windowWidthNormalized;

        // Make sure the target rect is parented to the slider so anchors are relative
        if (!floatingSlider.TryGetComponent<RectTransform>(out var sliderRect)) return;

        floatTargetArea.SetParent(sliderRect, false);

        // Set anchors to define the single window region on the slider (depending on slider direction)
        if (floatingSlider.direction == Slider.Direction.LeftToRight)
        {
            floatTargetArea.anchorMin = new Vector2(windowStartNormalized, 0f);
            floatTargetArea.anchorMax = new Vector2(windowEndNormalized, 1f);
            floatTargetArea.offsetMin = Vector2.zero;
            floatTargetArea.offsetMax = Vector2.zero;
        }
        else if (floatingSlider.direction == Slider.Direction.BottomToTop)
        {
            floatTargetArea.anchorMin = new Vector2(0f, windowStartNormalized);
            floatTargetArea.anchorMax = new Vector2(1f, windowEndNormalized);
            floatTargetArea.offsetMin = Vector2.zero;
            floatTargetArea.offsetMax = Vector2.zero;
        }

        if (showDebugLogs)
        {
            Debug.Log($"Rhythm window set: start={windowStartNormalized:F3}, width={windowWidthNormalized:F3}");
        }
    }

    private void Update()
    {
        if (Time.timeScale != 0f) 
        {
            HandleCooldown();

            if (!IsCoolingDown && canFloat)
            {
                // Start floating if input detected and not in inventory
                if (!IsFloating && !toggleInventoryUI.isEnabled && floatInput)
                {
                    StartFloating();
                    floatInput = false; // consume input
                }
            }

            if (IsFloating)
            {
                HandleRhythmInput();
                UpdateRhythmUI();
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsFloating)
        {
            ApplyFloatPhysics();
        }
    }

    private void StartFloating()
    {
        animator.ResetTrigger("isLanding");
        animator.SetBool("isFloating", false);
        animator.SetTrigger("floatStart");
        StartCoroutine(FloatAnimationHandler());

        if (chimeActive)
            chimeScript.setFloatingAnimation();

        floatingSlider.gameObject.SetActive(true);
        IsFloating = true;
        floatTimer = 0f;
        rhythmTimer = 0f;

        // randomize window for this float session so player sees a new target each time
        SetupRhythmTargets();

        prevRbUseGravity = rb.useGravity;
        prevRbDrag = rb.linearDamping;
        prevRbKinematic = rb.isKinematic;

        // enable rigidbody physics for floating
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearDamping = hoverDrag;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;

        // Apply lift force
        rb.AddForce(Vector3.up * floatLift, ForceMode.VelocityChange);

        // pick target hover height (from current world position)
        hoverTargetY = transform.position.y + floatHeightOffset;

        // disable controller systems
        if (playerController != null) playerController.enabled = false;
        if (charController != null) charController.enabled = false;

        // reset movement smoothing state
        currentMove = Vector3.zero;
        targetMove = Vector3.zero;
    }

    private void StopFloating()
    {
        animator.SetTrigger("isLanding");
        animator.SetBool("isFloating", false);

        if (chimeActive)
            chimeScript.ResetChimeAnimations();

        floatingSlider.gameObject.SetActive(false);
        IsFloating = false;
        rhythmTimer = 0f;
        floatTimer = 0f;
        if (floatingSlider != null) floatingSlider.value = 0f;

        IsCoolingDown = true;
        cooldownTimer = floatCooldown;

        // restore rigidbody settings
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = prevRbKinematic;
        rb.useGravity = prevRbUseGravity;
        rb.linearDamping = prevRbDrag;

        // re-enable controllers after syncing transform
        // ensure character/ player controllers see the current transform position
        if (charController != null) charController.enabled = true;
        if (playerController != null) playerController.enabled = true;

        canFloat = false;
    }

    private void HandleRhythmInput()
    {
        rhythmTimer += Time.deltaTime;
        if (rhythmTimer >= rhythmInterval)
        {
            // No successful input during cycle -> failure
            if (showDebugLogs) Debug.Log("Floating Rhythm Failure: rhythmTimer exceeded the interval");
            failState = true;
            StopFloating();
            return;
        }

        floatTimer += Time.deltaTime;

        if (floatTimer >= floatDuration)
        {
            StopFloating();
            if (showDebugLogs)
            {
                Debug.Log("Floating ended: ran out of time");
            }
            return;
        }

        if (floatInput && !toggleInventoryUI.isEnabled)
        {
            floatInput = false; // consume input

            // normalized progress across interval 0..1
            float timing = Mathf.Clamp01(rhythmTimer / rhythmInterval);

            // check if timing falls inside the success window
            if ((timing + rhythmBuffer) >= windowStartNormalized && (timing - rhythmBuffer) <= (windowStartNormalized + windowWidthNormalized))
            {
                // success
                rhythmTimer = 0f;
                if (floatingSlider != null)
                {
                    floatingSlider.value = 0f;
                }

                SetupRhythmTargets(); // new window for next input

                if (showDebugLogs)
                {
                    Debug.Log($"Floating Rhythm Success. Rhythm Timing = {timing:F3} | Success Window: {windowStartNormalized:F3}-{windowStartNormalized+windowWidthNormalized:F3} | Buffer: {rhythmBuffer}");
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"Floating Rhythm Failure. Rhythm Timing = {timing:F3} | Success Window: {windowStartNormalized:F3}-{windowStartNormalized + windowWidthNormalized:F3} | Buffer: {rhythmBuffer}");
                }
                // Immediate failure on incorrect input
                failState = true;
                StopFloating();
            }
        }
    }

    private void UpdateRhythmUI()
    {
        if (floatingSlider != null)
        {
            floatingSlider.value = Mathf.Clamp01(rhythmTimer / rhythmInterval);
        }
    }

    private void ApplyFloatPhysics()
    {
        // Get input relative to camera, apply deadzone and smoothing
        Vector3 raw = GetRawCameraRelativeInput(); // world-space direction (not normalized if zero)
        if (raw.sqrMagnitude < stickDeadzone * stickDeadzone)
        {
            // Apply 0 movement if within deadzone
            targetMove = Vector3.zero;
        }
        else
        {
            // normalized direction times speed
            targetMove = raw.normalized * horizontalSpeed;
        }

        // Smooth toward target
        currentMove = Vector3.Lerp(currentMove, targetMove, Mathf.Clamp01(moveSmoothing * Time.fixedDeltaTime));

        float oscillationAmplitude = 0.5f;
        float oscillationSpeed = 5f;
        float oscillation = Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmplitude;

        // Smoothly approach hover target Y
        float currentY = rb.position.y;
        float desiredY = Mathf.Lerp(currentY, hoverTargetY + oscillation, Mathf.Clamp01(verticalSmooth * Time.fixedDeltaTime));

        // Build next position
        Vector3 nextPos = rb.position + new Vector3(currentMove.x, 0f, currentMove.z) * Time.fixedDeltaTime;
        nextPos.y = desiredY;

        rb.MovePosition(nextPos);

        // Smoothly face movement direction
        Vector3 flatMove = new Vector3(currentMove.x, 0f, currentMove.z);
        if (flatMove.sqrMagnitude > 0.001f)
        {
            float targetAngle = Mathf.Atan2(flatMove.x, flatMove.z) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Mathf.Clamp01(moveSmoothing * Time.fixedDeltaTime));
        }
    }

    private Vector3 GetRawCameraRelativeInput()
    {
        // Get input values
        float x = moveInput.x;
        float z = moveInput.y;
        Vector3 input = new Vector3(x, 0f, z);

        // early out: no input
        if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

        // Get camera rotation vectors
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0f; camRight.Normalize();

        // combine 
        Vector3 world = camForward * input.z + camRight * input.x;
        return world;
    }

    private void HandleCooldown()
    {
        if (IsCoolingDown)
        {
            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(true);
                if (failState)
                {
                    // If the player failed the minigame, the cooldown fill is red
                    cooldownFillImage.color = Color.red;
                }
                else
                {
                    // Otherwise, if time ran out normally, cooldown fill is normal
                    cooldownFillImage.color = originalCooldownColor;
                }
                cooldownSlider.value = Mathf.Clamp01(1f - (cooldownTimer / floatCooldown));
            }
            
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                IsCoolingDown = false;
                failState = false;
            }
        }
        else if (cooldownSlider != null)
        {
            cooldownSlider.gameObject.SetActive(false);
            cooldownSlider.value = 0f;
        }
    }

    public void SetCanFloat (bool newCanfloat)
    {
        canFloat = newCanfloat;
    }

    IEnumerator FloatAnimationHandler()
    {
        if (!IsFloating)
        {
            ResetAnimations();
        }
        IsFloating = true;
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("isFloating", true);
    }

    private void ResetAnimations()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isGrabbing", false);
        animator.SetBool("isFloating", false);
    }
}
