using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFloating : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatLift = 3f;            // instantaneous upward boost on start
    [SerializeField] private float floatHeightOffset = 3f;   // height above start position to hover at
    [SerializeField] private float horizontalSpeed = 6f;     // max horizontal speed while floating
    [SerializeField] private float verticalSmooth = 5f;      // how quickly Y approaches target
    [SerializeField] private float moveSmoothing = 8f;       // how quickly horizontal motion interpolates
    [SerializeField] private float hoverDrag = 3f;           // extra drag while hovering
    [SerializeField] private float stickDeadzone = 0.2f;     // deadzone for controllers

    [Header("Rhythm / Input")]
    [SerializeField] private float floatDuration = 5f;
    [SerializeField] private float floatCooldown = 3f;
    [SerializeField] private KeyCode floatKey = KeyCode.Space;
    [SerializeField] private string floatButton = "Submit";
    [SerializeField] private Slider rhythmSlider;
    [SerializeField] private float rhythmWindow = 0.3f;
    [SerializeField] private float rhythmInterval = 1f;

    // internals
    private Rigidbody rb;
    private PlayerController playerController;
    private CharacterController charController;
    private Camera playerCamera;

    private bool isFloating = false;
    private float floatTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isCoolingDown = false;
    private float rhythmTimer = 0f;
    private float hoverTargetY;

    // smoothing state
    private Vector3 currentMove = Vector3.zero; // world-space horizontal velocity (x,z)
    private Vector3 targetMove = Vector3.zero;

    // restore original rigidbody/cc state
    //private bool prevCCEnabled;
    //private bool prevPlayerControllerEnabled;
    private bool prevRbUseGravity;
    private float prevRbDrag;
    private bool prevRbKinematic;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        charController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
    }

    private void Start()
    {
        rhythmSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleCooldown();

        if (!isCoolingDown)
        {
            if (!isFloating && (Input.GetKeyDown(floatKey) || Input.GetButtonDown(floatButton)))
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
            ApplyFloatPhysics();
    }

    private void StartFloating()
    {
        rhythmSlider.gameObject.SetActive(true);
        isFloating = true;
        floatTimer = 0f;
        rhythmTimer = 0f;

        // store previous states
        //if (charController != null) prevCCEnabled = charController.enabled;
        //if (playerController != null) prevPlayerControllerEnabled = playerController.enabled;
        prevRbUseGravity = rb.useGravity;
        prevRbDrag = rb.drag;
        prevRbKinematic = rb.isKinematic;

        // disable controller systems
        if (playerController != null) playerController.enabled = false;
        if (charController != null) charController.enabled = false;

        // enable rigidbody physics for floating
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.drag = hoverDrag;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        // lift up a bit
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
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = prevRbKinematic;
        rb.useGravity = prevRbUseGravity;
        rb.drag = prevRbDrag;

        // re-enable controllers AFTER syncing transform
        // ensure character/ player controllers see the current transform position
        if (charController != null) charController.enabled = true;
        if (playerController != null) playerController.enabled = true;
    }

    private void HandleRhythmInput()
    {
        rhythmTimer += Time.deltaTime;
        floatTimer += Time.deltaTime;

        if (floatTimer >= floatDuration)
        {
            StopFloating();
            Debug.Log("Floating ended: ran out of time");
            return;
        }

        if (Input.GetKeyDown(floatKey) || Input.GetButtonDown(floatButton))
        {
            float errorMargin = Mathf.Min(rhythmTimer, rhythmInterval - rhythmTimer);
            if (errorMargin <= rhythmWindow)
            {
                rhythmTimer = 0f;
                if (rhythmSlider != null) rhythmSlider.value = 0f;
                Debug.Log("Floating Rhythm Success");
            }
            else
            {
                Debug.Log("Floating failed: missed timing");
                StopFloating();
            }
        }

        if (rhythmTimer >= rhythmInterval + rhythmWindow)
        {
            Debug.Log("Floating failed: missed timing");
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
        // --- HORIZONTAL: get input relative to camera, apply deadzone and smoothing ---
        Vector3 raw = GetRawCameraRelativeInput(); // world-space direction (not normalized if zero)
        if (raw.sqrMagnitude < stickDeadzone * stickDeadzone)
        {
            // no meaningful input — target is zero
            targetMove = Vector3.zero;
        }
        else
        {
            // normalized direction times speed
            targetMove = raw.normalized * horizontalSpeed;
        }

        // smooth toward target
        currentMove = Vector3.Lerp(currentMove, targetMove, Mathf.Clamp01(moveSmoothing * Time.fixedDeltaTime));

        // --- VERTICAL: smoothly approach hover target Y ---
        float currentY = rb.position.y;
        float desiredY = Mathf.Lerp(currentY, hoverTargetY, Mathf.Clamp01(verticalSmooth * Time.fixedDeltaTime));

        // build next position
        Vector3 nextPos = rb.position + new Vector3(currentMove.x, 0f, currentMove.z) * Time.fixedDeltaTime;
        nextPos.y = desiredY;

        rb.MovePosition(nextPos);

        // --- ROTATION: smoothly face movement direction ---
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
        // raw axes (can be from gamepad or keyboard)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(x, 0f, z);

        // early out: no input
        if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

        // camera basis
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0f; camRight.Normalize();

        // combine (note: forward uses z, right uses x)
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
}
