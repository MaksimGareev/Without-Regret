using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    // Enum to define cardinal directions for the default facing direction of the camera, to be used by designers in the inspector
    public enum WorldDirection
    {
        North,
        South,
        East,
        West
    }
    
    // Not yet implemented, will apply appropriate vfx and movement settings
    // [Header("General Settings")]
    // [Tooltip("Set to true if this camera is used in the astral plane for special effects.")]
    // public bool isAstral = false; 
    // [Tooltip("Set to true if this camera is used indoors.")]
    // public bool isIndoors = false;

    [Header("Input")]
    [Tooltip("Insert a reference to the PlayerControls Input Action Asset")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction lookAction;

    [Header("Follow Settings")]
    [Tooltip("Default offset of the camera from the player (Setting this to (0,0,0) will equal the Player's exact transform). This will be rotated based on the default facing direction below.")]
    public Vector3 defaultOffset = new Vector3(0, 8, 8);

    [Tooltip("The offset of the position that the camera will aim at relative to the player (should be set to slightly above the player (y = 3)).")]
    public Vector3 defaultLookAtOffset = Vector3.zero;

    [Tooltip("The offeset used instead of default when charging a throw")]
    public Vector3 throwLookAtOffset = new Vector3(3,3,0);

    [Tooltip("Speed at which the camera moves to follow the player. Lower numbers are slower and smoother, higher numbers are faster and more rigid.")]
    public float smoothSpeed = 5f;
    private Transform target; // Reference the player as the intended target of the camera

    [Header("Default Facing Direction")]
    [Tooltip("Defines the world-space direction that the camera should face by default.")]
    [SerializeField] private WorldDirection defaultFacingDirection = WorldDirection.North;

    [Header("Camera Control Settings")]
    [Tooltip("Speed at which the camera rotates.")]
    [SerializeField] private float rotateSpeed = 120f;
    [Tooltip("Maximum pitch angle of the camera.")]
    [SerializeField] private float maxPitch = 45f;
    private float maxYaw = 120f;
    [Tooltip("Speed at which the camera returns to its default position.")]
    [SerializeField] private float returnSpeed = 4f;
    [Tooltip("Time in seconds after last mouse input before the camera starts returning to default.")]
    [SerializeField] private float mouseResetTime = 3f;
    [Tooltip("Scale factor for mouse rotation sensitivity.")]
    [SerializeField] private float mouseRotateScale = 0.08f;
    private bool rotateCamera = true;
    private bool restrictYaw = false;

    [Header("Focus Movement Settings")]
    [Tooltip("Offset applied to the camera when focusing on a pickup object.")]
    public Vector3 pickupOffset = new Vector3(3f, 2f, -5f);
    [Tooltip("Duration of the zoom effect when focusing on a pickup.")]
    public float zoomDuration = 2f;
    [Tooltip("Speed at which the camera transitions during focus movement.")]
    public float transitionSpeed = 2f;
    private bool isZooming = false;
    private Vector3 camPosCache = Vector3.zero;
    private Quaternion camRotCache = Quaternion.identity;
    private Vector3 lookAtCache = Vector3.zero;
    public bool CameraLocked { get; private set; }
    private Vector3 currentOffset;
    private Vector3 currentLookAtOffset;
    private float yaw;
    private float pitch;
    private Quaternion initialRotation;
    private ToggleInventoryUI toggleInventoryUI;
    private PlayerController pc;
    private float mouseResetTimer;
    private bool lastInputWasMouse = false;
    private PlayerController playerController;

    private void Awake()
    {
        // Set up input action references
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", true);
            lookAction = map.FindAction("Look", true);
        }
        else
        {
            Debug.LogWarning("InputActionAsset not assigned in CameraMovement.");
        }

    }

    private void OnEnable()
    {
        // Enable input and subscribe to scene change event
        lookAction?.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from event and disable input
        lookAction?.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Find the player and assign to target.
        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerController == null) // get player controller
        {
            playerController = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        }

        // Disable self if no player is found
        if (target == null)
        {
            Debug.LogWarning("Camera target not assigned, and no GameObject with tag 'Player' found. Disabling CameraMovement.");
            enabled = false;
            return;
        }

        // Find the inventory UI script and player controller script
        toggleInventoryUI = target.GetComponent<ToggleInventoryUI>();
        pc = target.GetComponent<PlayerController>();

        // Set initial camera position and rotation based on the default facing direction
        Vector3 facingVector = DirectionToVector(defaultFacingDirection);
        initialRotation = Quaternion.LookRotation(facingVector, Vector3.up);

        // Calculate the initial offset and lookAtOffset based on the default facing direction
        currentOffset = initialRotation * defaultOffset;
        currentLookAtOffset = initialRotation * defaultLookAtOffset;

        // Initialize yaw and pitch to 0 so that the camera starts at the default rotation
        yaw = 0f;
        pitch = 0f;

        // Set the initial position and rotation of the camera
        transform.position = target.position + currentOffset;
        transform.LookAt(target.position + currentLookAtOffset);

        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Delete this camera if it exists in the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Destroy(this);
        }
    }

    // Resets camera state when a new scene is loaded to prevent carrying over any state from the previous scene, such as being locked or zooming.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        CameraLocked = false;
        isZooming = false;
        yaw = 0f;
        pitch = 0f;
    }

    // Converts the WorldDirection enum to a corresponding Vector3 direction in world space
    private Vector3 DirectionToVector(WorldDirection direction)
    {
        switch (direction)
        {
            case WorldDirection.North:
                return Vector3.back;
            case WorldDirection.South:
                return Vector3.forward;
            case WorldDirection.East:
                return Vector3.left;
            case WorldDirection.West:
                return Vector3.right;
            default:
                return Vector3.forward;
        }
    }

    void LateUpdate()
    {
        // Do nothing if camera is locked or there is no target assigned
        if (CameraLocked && lookAction != null && lookAction.enabled)
        {
            lookAction?.Disable();
            return;
        }
        else if (!CameraLocked && lookAction != null && !lookAction.enabled)
        {
            lookAction?.Enable();
        }
        
        if (target == null) return;
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOver) return;

        if (pc != null && pc.MovementLocked && pc.enabled)
        {
            pc.enabled = false;
        }

        // Read look input
        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        // Determine if the last input was from the mouse based on whether there is significant look input and the current control scheme
        if (lookInput.sqrMagnitude > 0.0001f && lookAction != null && lookAction.activeControl.device is Mouse)
        {
            lastInputWasMouse = true;
        }
        else
        {
            lastInputWasMouse = false;
        }

        // Check if there is significant look input to determine whether to rotate the camera or return to default position, and to reset the mouse timer
        bool hasLookInput = lookInput.sqrMagnitude > 0.0001f;

        if (!hasLookInput && mouseResetTimer >= 0)
        {
            mouseResetTimer -= Time.deltaTime;
        }

        // Determine if camera control should be blocked based on whether the inventory UI is open or the player controller has movement locked
        bool blocked = (toggleInventoryUI != null && toggleInventoryUI.isEnabled) || (pc != null && pc.MovementLocked);

        // Only allow camera movement if not zooming
        if (!isZooming)
        {
            if (!blocked && hasLookInput)
            {
                // If input is detected, call functions to apply rotation and handle the reset timer
                float h = -lookInput.x;
                float v = -lookInput.y;

                if (rotateCamera)
                {
                    HandleRotation(h, v, lastInputWasMouse);
                }

                if (lastInputWasMouse)
                {
                    mouseResetTimer = mouseResetTime;
                }
            }
            else
            {
                // Return the camera to its default position and rotation
                if (rotateCamera)
                {
                    ReturnRotation();
                }
            }
            
            // Position of the camera
            Vector3 desiredPosition = target.position + currentOffset;

            // Smooth following of the player
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            Vector3 lookAtPos = target.position + currentLookAtOffset;

            // Look at the Player
            transform.LookAt(lookAtPos);
            
        } 
    }

    // Handles camera rotation based on input, with separate handling for mouse and controller input. 
    // Both mouse and controller input are scaled by separate sensitivity settings
    // Yaw can be optionally restricted, and pitch is always restricted to prevent flipping.
    private void HandleRotation(float horizontalInput, float verticalInput, bool isMouse)
    {
        // Scale input based on whether it's mouse or controller, and apply to yaw and pitch
        if (isMouse)
        {
            float mouseScale = mouseRotateScale * GameSettings.MouseSensitivity / 100f;
            yaw -= horizontalInput * mouseScale;
            pitch -= verticalInput * mouseScale;
        }
        else
        {
            float stickScale = GameSettings.RightStickSensitivity / 100f;
            yaw -= horizontalInput * rotateSpeed * stickScale * Time.deltaTime;
            pitch -= verticalInput * rotateSpeed * stickScale * Time.deltaTime;
        }

        // Apply restrictions to yaw, otherwise wrap it around smoothly
        if (restrictYaw)
        {
            yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        }
        else
        {
            yaw = Mathf.Repeat(yaw + 180f, 360f) - 180f;
        }

        // Clamp pitch to prevent flipping
        pitch = Mathf.Clamp(pitch, -Mathf.Abs(maxPitch), Mathf.Abs(maxPitch));

        // Calculate the new rotation based on the initial rotation and the current yaw and pitch
        Quaternion rotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);

        // Update the current offset and lookAtOffset based on the new rotation
        currentOffset = rotation * defaultOffset;

        if (!playerController.isThrowing)
        {
            currentLookAtOffset = rotation * defaultLookAtOffset;
        }
        else
        {
            currentLookAtOffset = rotation * throwLookAtOffset;
        }
    }
    
    // Smoothly returns the camera to its default position and rotation when there is no input for a certain amount of time
    private void ReturnRotation()
    {
        if (mouseResetTimer >= 0f)
        {
            return;
        }

        if (restrictYaw)
        {
            yaw = Mathf.Lerp(yaw, 0f, returnSpeed * Time.deltaTime);
        }
        
        pitch = Mathf.Lerp(pitch, 0f, returnSpeed * Time.deltaTime);

        Quaternion rotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);
        currentOffset = rotation * defaultOffset;

        if (!playerController.isThrowing)
        {
            currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, initialRotation * defaultLookAtOffset, returnSpeed * Time.deltaTime);
        }
        else
        {
            currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, initialRotation * throwLookAtOffset, returnSpeed * Time.deltaTime);
        }
    }

    // Public function to be called when a dialogue trigger is activated to start the camera zoom effect
    public void TriggerDialogueCamera(Transform dialogueTrigger)
    {
        if (!isZooming)
        {
            StartCoroutine(StartCameraZoom(dialogueTrigger, true));
        }
    }

    // Ends the camera zoom effect and releases control back to the normal camera movement
    public IEnumerator EndCameraZoom()
    {
        if (camPosCache == Vector3.zero || camRotCache == Quaternion.identity || lookAtCache == Vector3.zero)
        {
            yield break;
        }

        // TO FIX: With this implementation, the camera will snap back to the original position and rotation at the end of the coroutine
        // Because LateUpdate will override the position and rotation once it finishes.
        // Commenting this out for now so it's a little less jarring, but a smooth transition would be ideal later

        // transform.GetPositionAndRotation(out Vector3 currentPos, out Quaternion currentRot);
        // float t = 0;
        // while (t < zoomDuration)
        // {
        //    t += Time.deltaTime * transitionSpeed;
        //    transform.SetPositionAndRotation(Vector3.Lerp(currentPos, camPosCache, t), Quaternion.Slerp(currentRot, camRotCache, t));
        //    Vector3 lookAtPos = Vector3.Lerp(lookAtCache, target.position + currentLookAtOffset, t);
        //    transform.LookAt(lookAtPos);
        //    yield return null;
        // }

        isZooming = false;
        CameraLocked = false;

        camPosCache = Vector3.zero;
        camRotCache = Quaternion.identity;
        lookAtCache = Vector3.zero;
    }

    // Public function to be called when an item is picked up to trigger the camera zoom
    public void TriggerPickupCameraEffect(Transform item)
    {
        if (!isZooming)
        {
            StartCoroutine(StartCameraZoom(item, false));
            StartCoroutine(PauseZoomForItem());
        }
    }

    // Simple coroutine to pause the camera zoom when focusing on a pickup item, then trigger the end zoom function
    IEnumerator PauseZoomForItem()
    {
        yield return new WaitForSeconds(zoomDuration / 2f);
        StartCoroutine(EndCameraZoom());
    }

    // This coroutine smoothly moves the camera to focus on a specific target (pickup or dialogue trigger), then holds until EndCameraZoom is called. 
    // If dialogue is true, it will use a different offset for the camera position.
    public IEnumerator StartCameraZoom(Transform zoomTarget, bool dialogue = false)
    {
        CameraLocked = true;
        isZooming = true;

        // Cache current camera transform
        camPosCache = transform.position;
        camRotCache = transform.rotation;

        // Direction the player is facing
        Vector3 playerForward = zoomTarget.position - target.position;
        playerForward.y = 0f;
        playerForward.Normalize();

        // Offset relative to the player
        Vector3 offset;
        if (dialogue)
        {
            // Offset behind player, up, and to the right
            offset = -playerForward * 3f + target.right * 3f + Vector3.up * 2f;
        }
        else
        {
            offset = pickupOffset;
        }

        // Target position
        Vector3 targetPos = target.position + offset;

        // Target Rotation
        Quaternion targetRot = Quaternion.LookRotation(zoomTarget.position - targetPos);

        lookAtCache = zoomTarget.position;

        // Smoothly move and rotate the camera to the target position and rotation
        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime * transitionSpeed;
            transform.position = Vector3.Lerp(camPosCache, targetPos, t);
            transform.rotation = Quaternion.Slerp(camRotCache, targetRot, t);
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);
    }

    // Setter function for other scripts to lock the camera
    public void SetCameraLocked(bool locked)
    {
        CameraLocked = locked;
    }

}
