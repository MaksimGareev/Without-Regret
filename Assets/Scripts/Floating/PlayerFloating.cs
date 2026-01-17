using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFloating : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatLift = 3f; // instantaneous upward boost on start
    [SerializeField] private float floatHeightOffset = 3f; // height above start position to hover at
    [SerializeField] private float horizontalSpeed = 6f; // max horizontal speed while floating
    [SerializeField] private float verticalSmooth = 5f; // how quickly Y approaches target
    [SerializeField] private float moveSmoothing = 8f; // how quickly horizontal motion interpolates
    [SerializeField] private float hoverDrag = 3f; // extra drag while hovering
    [SerializeField] private float stickDeadzone = 0.2f; // deadzone for controllers

    [Header("Rhythm / Input")]
    [SerializeField] private InputActionReference floatAction;
    [SerializeField] private float floatDuration = 5f;
    [SerializeField] private float floatCooldown = 3f;
    [SerializeField] private KeyCode floatKey = KeyCode.Space;
    [SerializeField] private string floatButton = "Submit";
    [SerializeField] private Slider rhythmSlider;
    [SerializeField] private float rhythmWindow = 0.3f;
    [SerializeField] private float rhythmInterval = 1f;
    private PlayerControls controls;
    private Vector2 moveInput;
    private bool floatInput;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private Rigidbody rb;
    private PlayerController playerController;
    private CharacterController charController;
    private Camera playerCamera;
    private ToggleInventoryUI toggleInventoryUI;

    public bool isFloating { get; private set; } = false;
    private bool canFloat = false;
    private float floatTimer = 0f;
    private float cooldownTimer = 0f;
    public bool isCoolingDown { get; private set; } = false;
    private float rhythmTimer = 0f;
    private float hoverTargetY;

    private Vector3 currentMove = Vector3.zero;
    private Vector3 targetMove = Vector3.zero;

    private bool prevRbUseGravity;
    private float prevRbDrag;
    private bool prevRbKinematic;

    void OnEnable()
    {
        controls.Enable();
        floatAction.action.Enable();
    }
    void OnDisable()
    {
        controls.Disable();
        floatAction.action.Disable();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        charController = GetComponent<CharacterController>();
        toggleInventoryUI = GetComponent<ToggleInventoryUI>();
        playerCamera = Camera.main;
        controls = new PlayerControls();

        // Assign floatInput based on the state of the Input Action
        floatAction.action.performed += ctx => OnSubmit(ctx);
        floatAction.action.canceled += ctx => OnSubmit(ctx);
        controls.Player.Move.performed += ctx => OnMove(ctx);
        controls.Player.Move.canceled += ctx => OnMove(ctx);

        if (floatAction == null)
        {
            Debug.LogError("PlayerFloating: Float Input Action Reference is not assigned.");
        }
    }

    void OnSubmit(InputAction.CallbackContext context)
    {
        floatInput = context.action.triggered;

        //if (showDebugLogs)
        //{
        //    Debug.Log("Float Input: " + floatInput);
        //}
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        //if (showDebugLogs && isFloating)
        //{
        //    Debug.Log("PlayerFloating - Move Input: " + moveInput);
        //}
    }

    private void Start()
    {
        rhythmSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale != 0f) 
        {
            HandleCooldown();

            if (!isCoolingDown && canFloat)
            {
                // Start floating if input detected and not in inventory
                if (!isFloating && !toggleInventoryUI.isEnabled && floatInput)
                {
                    StartFloating();
                }
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

    private void StartFloating()
    {
        rhythmSlider.gameObject.SetActive(true);
        isFloating = true;
        floatTimer = 0f;
        rhythmTimer = 0f;

        prevRbUseGravity = rb.useGravity;
        prevRbDrag = rb.linearDamping;
        prevRbKinematic = rb.isKinematic;

        // disable controller systems
        if (playerController != null) playerController.enabled = false;
        if (charController != null) charController.enabled = false;

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

        // reset movement smoothing state
        currentMove = Vector3.zero;
        targetMove = Vector3.zero;
    }

    private void StopFloating()
    {
        rhythmSlider.gameObject.SetActive(false);
        isFloating = false;
        rhythmTimer = 0f;
        floatTimer = 0f;
        if (rhythmSlider != null) rhythmSlider.value = 0f;

        isCoolingDown = true;
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
            float errorMargin = Mathf.Min(rhythmTimer, rhythmInterval - rhythmTimer);
            if (errorMargin <= rhythmWindow)
            {
                rhythmTimer = 0f;
                if (rhythmSlider != null)
                {
                    rhythmSlider.value = 0f;
                }

                if (showDebugLogs)
                {
                    Debug.Log("Floating Rhythm Success");
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log("Floating failed: missed timing");
                }
                StopFloating();
            }
        }

        if (rhythmTimer >= rhythmInterval + rhythmWindow)
        {
            if (showDebugLogs)
            {
                Debug.Log("Floating failed: missed timing");
            }
            StopFloating();
        }
    }

    private void UpdateRhythmUI()
    {
        if (rhythmSlider != null)
        {
            rhythmSlider.value = Mathf.Clamp01(rhythmTimer / rhythmInterval);
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
        //float x = Input.GetAxis("Horizontal");
        //float z = Input.GetAxis("Vertical");
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
        if (isCoolingDown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f) isCoolingDown = false;
        }
    }

    public void SetCanFloat (bool newCanfloat)
    {
        canFloat = newCanfloat;
    }
}
