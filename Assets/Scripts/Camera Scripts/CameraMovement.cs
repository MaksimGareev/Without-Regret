using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
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
    [Header("Astral Post Processing")]
    [Tooltip("Set to true if this camera is used in the astral plane for special effects. This will enable the post-processing effects for the Astral Plane.")]
    public bool isAstral = false;
    
    [Tooltip("Reference to the GameObject containing the VFX for the astral plane, which will be toggled on and off based on the isAstral boolean. The object should be a child of the Main Camera Prefab")]
    [SerializeField] private GameObject astralVFX;

    [Header("Input")]
    [Tooltip("Insert a reference to the PlayerControls Input Action Asset")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction lookAction;

    [Header("Follow Movement Settings")]

    [Tooltip("If true, the camera will follow the player. If false, the camera will remain in the position it is placed in the editor, or the position it is at the time this boolean is set false.")]
    [SerializeField] private bool followPlayer = true;

    [SerializeField] private bool checkCollisions = false;

    [Tooltip("Which layers will be ignored when checking for collisions.")]
    [SerializeField] private LayerMask ignoreCollisionLayer;

    [Tooltip("Default offset of the camera from the player (Setting this to (0,0,0) will equal the Player's exact transform). This will be rotated based on the default facing direction below.")]
    public Vector3 defaultOffset = new Vector3(0, 8, 8);

    [Tooltip("The offset of the position that the camera will aim at relative to the player (should be set to slightly above the player (y = 3)).")]
    public Vector3 defaultLookAtOffset = Vector3.zero;

    [Tooltip("The offeset used instead of default when charging a throw")]
    public Vector3 throwLookAtOffset = new Vector3(3, 3, 0);

    [Tooltip("Speed at which the camera moves to follow the player. Lower numbers are slower and smoother, higher numbers are faster and more rigid.")]
    [SerializeField] private float smoothSpeed = 5f;
    private Transform target; // Reference the player as the intended target of the camera

    [Header("Rotation Settings")]
    [Tooltip("Defines the world-space direction that the camera should face by default.")]
    [SerializeField] private WorldDirection defaultFacingDirection = WorldDirection.North;

    [Tooltip("If true, the camera will rotate based on player input. If false, the camera will remain fixed behind the player.")]
    [SerializeField] private bool rotateCamera = true;

    [Tooltip("Speed at which the camera rotates.")]
    [SerializeField] private float rotateSpeed = 120f;

    [Tooltip("Speed at which the camera returns to its default position.")]
    [SerializeField] private float returnSpeed = 4f;

    [Tooltip("Time in seconds after last mouse input before the camera starts returning to default.")]
    [SerializeField] private float mouseResetTime = 3f;

    [Tooltip("Scale factor for mouse rotation sensitivity.")]
    [SerializeField] private float mouseRotateScale = 0.08f;

    [Tooltip("If true, the camera's yaw will be restricted to the maxYaw angle. If false, the camera can rotate freely around the player.")]
    [SerializeField] private bool restrictYaw = false;

    [Tooltip("Maximum yaw angle of the camera when restrictYaw is enabled.")]
    [SerializeField] private float maxYaw = 120f;

    [Tooltip("Maximum pitch angle of the camera.")]
    [SerializeField] private float maxPitch = 45f;

    [Header("Focus Movement Settings")]
    [Tooltip("Offset applied to the camera when focusing on a pickup object.")]
    [SerializeField] private Vector3 pickupOffset = new Vector3(3f, 2f, -5f);

    [Tooltip("Duration of the zoom effect when focusing on a pickup.")]
    [SerializeField] private float zoomDuration = 2f;

    [Tooltip("Speed at which the camera transitions during focus movement.")]
    [SerializeField] private float transitionSpeed = 2f;
    
    [SerializeField] private Transform ThrowTarget;

    [Header("Return Blending")]
    [Tooltip("Duration used when smoothly blending the camera back to its normal follow position/rotation after an override.")]
    [SerializeField] private float returnBlendDuration = 0.6f;

    // Cache settings for returning after override triggers
    private Vector3 cachedOffset;
    private Vector3 cachedLookAtOffset;
    private bool cachedFollowPlayer;
    private float cachedSmoothSpeed;
    private bool cachedRotateCamera;
    private float cachedRotateSpeed;
    private bool cachedRestrictYaw;
    private float cachedMaxYaw;
    private float cachedMaxPitch;

    // bools to flag if settings were overridden
    private bool positionOverridden = false;
    private bool followOverridden = false;
    private bool rotationOverridden = false;

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

    // Camera shake state
    private Vector3 shakeOffset = Vector3.zero;
    private Coroutine shakeRoutine = null;

    // Camera look-at state
    private Coroutine lookRoutine = null;
    private bool isLookingAtSubject = false;
    private bool moveCameraToPosition = false;
    private Transform moveToPosition;

    // Blending state - used to smoothly interpolate back to normal follow position/rotation
    private Coroutine blendRoutine = null;
    private bool isBlendingToNormal = false;

    // When true the normal follow position is suppressed (prevents LateUpdate from snapping camera back while we move+hold)
    private bool suppressFollow = false;

    private bool cameraInputEnabled = true;

    private float lerpBetweenValue = 0f;
    private float lerpSpeed = 1.5f;
    
    private AudioSource shakeAudioSource;
    public AudioMixer mainAudioMixer;

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

        if (isAstral && astralVFX != null)
        {
            astralVFX.SetActive(true);
        }
        else if (astralVFX != null)
        {
            astralVFX.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Astral VFX GameObject reference not set in CameraMovement.");
        }

        if (checkCollisions && !TryGetComponent<Collider>(out _))
        {
            Debug.LogWarning("Check Collisions is enabled but no Collider component found on the camera. Disabling collision checking.");
            checkCollisions = false;
        }

        if (ObjectiveManager.Instance)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(ShakeOnObjectiveActive);
        }
        
        shakeAudioSource = GetComponent<AudioSource>();
        if (shakeAudioSource)
        {
            shakeAudioSource.outputAudioMixerGroup = mainAudioMixer.FindMatchingGroups("SFX")[0];
            shakeAudioSource.volume = 0.6f;
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
        StopAllCoroutines();
    }

    private void Start()
    {
        // Find the player and assign to target.
        target = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerController == null) // get player controller
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        if (ThrowTarget == null)
        {
            ThrowTarget = playerController.throwLookatPoint;
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
        if (followPlayer)
        {
            transform.position = target.position + currentOffset;
            transform.LookAt(target.position + currentLookAtOffset);
        }

        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Delete this camera if it exists in the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Destroy(this);
        }

        cachedOffset = defaultOffset;
        cachedLookAtOffset = defaultLookAtOffset;
        cachedFollowPlayer = followPlayer;
        cachedSmoothSpeed = smoothSpeed;
        cachedRotateSpeed = rotateSpeed;
        cachedRotateCamera = rotateCamera;
        cachedRestrictYaw = restrictYaw;
        cachedMaxYaw = maxYaw;
        cachedMaxPitch = maxPitch;
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
        // Manage enabling/disabling of lookAction when camera is completely locked
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

        if (GameManager.Instance.pauseManager != null
            && GameManager.Instance.pauseManager.GetComponent<PauseManager>() != null
            && GameManager.Instance.pauseManager.GetComponent<PauseManager>().isGamePaused) return;

        if (GameManager.Instance.journalUICanvas != null
            && GameManager.Instance.journalUICanvas.GetComponent<Journal>() != null
            && GameManager.Instance.journalUICanvas.GetComponent<Journal>().IsJournalOpen) return;

        if (GameManager.Instance.LockPickUI != null
            && GameManager.Instance.LockPickUI.GetComponent<LockPickUI>() != null
            && GameManager.Instance.LockPickUI.GetComponent<LockPickUI>().IsActive) return;

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
        bool hasLookInput = lookInput.sqrMagnitude > 0.001f;

        if (!hasLookInput && mouseResetTimer >= 0)
        {
            mouseResetTimer -= Time.deltaTime;
        }

        // Determine if camera control should be blocked based on whether the inventory UI is open or the player controller has movement locked
        bool blocked = (toggleInventoryUI != null && toggleInventoryUI.isEnabled) || (pc != null && pc.MovementLocked);

        // Only allow camera movement if not zooming AND camera input is enabled AND we're not actively locked to look at a subject.
        if (!isZooming && cameraInputEnabled && !isLookingAtSubject)
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
                    //ReturnRotation();
                }
            }
        }

        // Early return if the camera is not set to follow the player, allowing it to remain fixed in place
        if (!followPlayer) return;

        // Position of the camera
        Vector3 desiredPosition = target.position + currentOffset;

        // Smooth following of the player
        Vector3 movePosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // When blending to normal is active or while a zoom is running, do not overwrite transform.position here;
        // the blend coroutine or zoom coroutine drives transform until blending completes or zoom ends.
        // Also respect suppressFollow so LookAt routines that temporarily move the camera are not overwritten.
        if (!isZooming && !isBlendingToNormal && !suppressFollow)
        {
            if (checkCollisions)
            {
                if (CanMoveBy(movePosition - transform.position))
                {
                    // apply procedural shake offset on top of the computed follow position
                    transform.position = movePosition + shakeOffset;
                }
            }
            else
            {
                // apply procedural shake offset on top of the computed follow position
                transform.position = movePosition + shakeOffset;
            }
        }

        Vector3 lookAtPos;
        if (!playerController.isThrowing || ThrowTarget == null)
        {
            if(lerpBetweenValue > 0)
            {
                lerpBetweenValue -= Time.deltaTime * lerpSpeed;
            }
        }
        else
        {
            if(lerpBetweenValue < 1)
            {
                lerpBetweenValue += Time.deltaTime * lerpSpeed;
            }
        }

        lookAtPos = Vector3.Lerp(target.position + currentLookAtOffset, ThrowTarget.position + currentLookAtOffset, lerpBetweenValue);
        // If we're actively looking at a subject, do not override rotation with the default LookAt.
        // Also if blending to normal or zooming, the coroutine will control rotation.
        if (!isLookingAtSubject && !isBlendingToNormal && !isZooming)
        {
            // Look at the Player
            transform.LookAt(lookAtPos);
        }
    }
    
    private void ShakeOnObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data.cameraShake)
        {
            Shake(objective.data.cameraShakeDuration, objective.data.cameraShakeMagnitude, objective.data.cameraShakeFrequency);
            
            if (objective.data.shakeSound)
            {
                shakeAudioSource.PlayOneShot(objective.data.shakeSound);
            }
        }
    }

    /// <summary>
    /// Causes the camera to shake.
    /// </summary>
    /// <param name="duration">How long the camera shakes for</param>
    /// <param name="magnitude">The 'strength' of each shake</param>
    /// <param name="frequency">How often a shake occurs</param>
    public void Shake(float duration, float magnitude, float frequency = 30f)
    {
        // stop any existing shake then start new
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
            shakeOffset = Vector3.zero;
        }

        if (duration <= 0f || magnitude <= 0f)
        {
            return;
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude, frequency));
    }

    // Immediately stop any ongoing shake and clear offset
    public void StopShake()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }
        shakeOffset = Vector3.zero;
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude, float frequency)
    {
        float elapsed = 0f;
        float interval = 1f / Mathf.Max(1f, frequency);

        // lerp the random offsets to make the shake smoother
        Vector3 prev = Vector3.zero;
        Vector3 target = Vector3.zero;
        float t = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // advance interpolation between random targets
            t += Time.deltaTime / interval;
            if (t >= 1f)
            {
                // pick new random target (bias to horizontal plane, small vertical)
                prev = target;
                Vector2 rand = Random.insideUnitCircle;
                target = new Vector3(rand.x, rand.y * 0.35f, 0f) * magnitude;
                t = 0f;
            }

            // smoothstep interpolation for nicer feel
            float s = Mathf.SmoothStep(0f, 1f, t);
            shakeOffset = Vector3.Lerp(prev, target, s);

            yield return null;
        }

        // currentLookAtOffset = rotation * defaultLookAtOffset;
        // decay to zero smoothly
        float decayTime = 0.15f;
        float dec = 0f;
        Vector3 start = shakeOffset;
        while (dec < decayTime)
        {
            dec += Time.deltaTime;
            shakeOffset = Vector3.Lerp(start, Vector3.zero, dec / decayTime);
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeRoutine = null;
    }

    // Public function to enable / disable camera input
    public void SetCameraInputEnabled(bool enabled)
    {
        cameraInputEnabled = enabled;

        // reset mouse timer so camera doesn't immediately jump when re-enabled
        if (enabled)
        {
            mouseResetTimer = mouseResetTime;
        }
    }

    /// <summary>
    /// Initiates a camera look at the specified subject, optionally moving the camera to a target position and
    /// disabling input during the look. The camera can automatically return to its normal pose after a specified
    /// duration or remain focused until manually reset.
    /// </summary>
    /// <remarks>Calling this method interrupts any ongoing look or blend-to-normal operation. If input is
    /// disabled, it is re-enabled when the look ends. To manually end the look when <paramref name="autoReturn"/> is
    /// <see langword="false"/>, call <c>StopLookingAtSubject()</c>.</remarks>
    /// <param name="subject">The transform to look at. Cannot be null.</param>
    /// <param name="cameraMoveTo">An optional transform specifying the position to which the camera should move before and during the look. If
    /// null, the camera remains at its current position.</param>
    /// <param name="rotateDuration">The duration, in seconds, over which the camera rotates and moves into position. If less than or equal to zero,
    /// the transition occurs immediately.</param>
    /// <param name="disableCameraInputWhileLooking">If <see langword="true"/>, camera rotation input is disabled while the look is active.</param>
    /// <param name="disablePlayerInputWhileLooking">If <see langword="true"/>, player input is disabled while the look is active.</param>
    /// <param name="holdDuration">The duration, in seconds, to hold the camera's focus on the subject before automatically returning. Used only if
    /// <paramref name="autoReturn"/> is <see langword="true"/>.</param>
    /// <param name="autoReturn">If <see langword="true"/>, the camera automatically returns to its normal follow pose after <paramref
    /// name="holdDuration"/> seconds. If <see langword="false"/>, the camera remains focused on the subject until
    /// <c>StopLookingAtSubject()</c> is called.</param>
    public void LookAtSubject(Transform subject, Transform cameraMoveTo = null, float rotateDuration = 1f, bool disableCameraInputWhileLooking = true, bool disablePlayerInputWhileLooking = false, float holdDuration = 0.6f, bool autoReturn = true)
    {
        if (subject == null) return;

        // If a cameraMoveTo transform is provided, flag it so the coroutine will move the camera there before (and while) rotating
        moveCameraToPosition = (cameraMoveTo != null);
        moveToPosition = cameraMoveTo;

        // stop any existing look coroutine first
        if (lookRoutine != null)
        {
            StopCoroutine(lookRoutine);
            lookRoutine = null;
        }

        // If a blend back to normal is running, stop it before starting a look
        if (blendRoutine != null)
        {
            StopCoroutine(blendRoutine);
            blendRoutine = null;
            isBlendingToNormal = false;
        }

        lookRoutine = StartCoroutine(LookAtRoutine(subject, rotateDuration, disableCameraInputWhileLooking, disablePlayerInputWhileLooking, holdDuration, autoReturn, moveCameraToPosition, moveToPosition));
    }

    // Stop an ongoing look-at and smoothly return to the normal follow pose.
    // Camera input will be restored after the blend completes.
    public void StopLookingAtSubject()
    {
        if (lookRoutine != null)
        {
            StopCoroutine(lookRoutine);
            lookRoutine = null;
        }

        // mark that we should no longer hold the subject
        isLookingAtSubject = false;

        // Begin smooth blend back to the normal follow pose and re-enable input at the end
        StartBlendToNormal(returnBlendDuration, reenableCameraAtEnd: true, reenablePlayerAtEnd: true);
    }

    private IEnumerator LookAtRoutine(Transform subject, float rotateDuration, bool disableInputWhileLooking, bool disablePlayerInputWhileLooking, float holdDuration, bool autoReturn, bool moveCamera, Transform cameraMoveTo)
    {
        // disable camera input if requested
        if (disableInputWhileLooking)
            SetCameraInputEnabled(false);

        if (disablePlayerInputWhileLooking && target.TryGetComponent<PlayerController>(out var player))
            player.DisableInput();

        isLookingAtSubject = true;

        // If we are instructed to move the camera to a provided transform, smoothly move & rotate there over rotateDuration.
        if (moveCamera && cameraMoveTo != null)
        {
            // suppress follow so LateUpdate doesn't snap the camera back while we move & hold
            suppressFollow = true;

            transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
            Vector3 targetPos = cameraMoveTo.position;

            float elapsedMove = 0f;
            float durMove = Mathf.Max(0.0001f, rotateDuration);

            while (elapsedMove < durMove && isLookingAtSubject)
            {
                elapsedMove += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedMove / durMove);
                float s = Mathf.SmoothStep(0f, 1f, t);

                // Interpolate position
                Vector3 curPos = Vector3.Lerp(startPos, targetPos, s);
                transform.position = curPos;

                // Interpolate rotation toward subject so camera arrives facing them
                Vector3 dir = subject.position - curPos;
                if (dir.sqrMagnitude > 0.000001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(startRot, targetRot, s);
                }

                yield return null;
            }

            // Ensure final pose
            Vector3 finalDir = subject.position - targetPos;
            if (finalDir.sqrMagnitude > 0.000001f)
            {
                transform.SetPositionAndRotation(targetPos, Quaternion.LookRotation(finalDir.normalized, Vector3.up));
            }
            else
            {
                transform.position = targetPos;
            }

            // If autoReturn is requested we should remain at the targetPos for holdDuration, then start blending back.
            if (autoReturn)
            {
                // remain at the moved position and continue looking at the subject for the hold duration
                float wait = Mathf.Max(0f, holdDuration);
                float elapsedWait = 0f;
                while (elapsedWait < wait && isLookingAtSubject)
                {
                    elapsedWait += Time.deltaTime;
                    // keep facing the subject while waiting in case they move
                    Vector3 dir = subject.position - transform.position;
                    if (dir.sqrMagnitude > 0.000001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                    }
                    yield return null;
                }

                isLookingAtSubject = false;

                // start blend back to normal; BlendToNormalCoroutine will clear suppressFollow at the end
                StartBlendToNormal(returnBlendDuration, reenableCameraAtEnd: disableInputWhileLooking, reenablePlayerAtEnd: disablePlayerInputWhileLooking);
                lookRoutine = null;
                yield break;
            }
            else
            {
                // Keep facing the subject until StopLookingAtSubject is called
                while (isLookingAtSubject)
                {
                    Vector3 dir = subject.position - transform.position;
                    if (dir.sqrMagnitude > 0.000001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                    }
                    yield return null;
                }

                // StopLookingAtSubject will trigger blending back
                lookRoutine = null;
                yield break;
            }
        }

        // Capture starting rotation
        Quaternion startRotation = transform.rotation;

        float elapsed = 0f;
        // If rotateDuration is zero or negative, snap immediately
        if (rotateDuration <= 0f)
        {
            Vector3 dirSnap = (subject.position - transform.position);
            if (dirSnap.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dirSnap.normalized, Vector3.up);

            // If autoReturn is requested, wait holdDuration then blend back; otherwise keep looking until StopLookingAtSubject
            if (autoReturn)
            {
                yield return new WaitForSeconds(Mathf.Max(0f, holdDuration));
                isLookingAtSubject = false;
                StartBlendToNormal(returnBlendDuration, reenableCameraAtEnd: disableInputWhileLooking, reenablePlayerAtEnd: disablePlayerInputWhileLooking);
            }
            else
            {
                while (isLookingAtSubject)
                {
                    Vector3 dir = subject.position - transform.position;
                    if (dir.sqrMagnitude > 0.000001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                    }
                    yield return null;
                }
            }

            lookRoutine = null;
            yield break;
        }

        // Smoothly rotate toward a dynamic target rotation computed each frame
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotateDuration);

            // compute dynamic target rotation so camera will follow a moving subject smoothly
            Vector3 dir = subject.position - transform.position;
            if (dir.sqrMagnitude <= 0.000001f) { yield return null; continue; }

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

            // Interpolate from startRotation towards the dynamic target
            transform.rotation = Quaternion.Slerp(startRotation, targetRot, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        // After finished rotating:
        if (autoReturn)
        {
            // hold for selected time, then blend back to normal automatically
            float hold = Mathf.Max(0f, holdDuration);
            float wait = 0f;
            while (wait < hold)
            {
                // continue updating rotation toward subject while holding
                Vector3 dir = subject.position - transform.position;
                if (dir.sqrMagnitude > 0.000001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                }

                wait += Time.deltaTime;
                yield return null;
            }

            // stop holding and blend back to normal
            isLookingAtSubject = false;
            StartBlendToNormal(returnBlendDuration, reenableCameraAtEnd: disableInputWhileLooking, reenablePlayerAtEnd: disablePlayerInputWhileLooking);
        }
        else
        {
            // Keep facing the subject until StopLookingAtSubject is called
            while (isLookingAtSubject)
            {
                Vector3 dir = subject.position - transform.position;
                if (dir.sqrMagnitude > 0.000001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                }
                yield return null;
            }

            // If the loop exits because StopLookingAtSubject was called, StartBlendToNormal() is already triggered by that caller.
        }

        lookRoutine = null;
    }

    // Starts a coroutine that blends camera transform from its current position/rotation to the computed normal follow pose.
    // reenableInputAtEnd: if true, camera input will be restored when the blend completes.
    private void StartBlendToNormal(float duration, bool reenableCameraAtEnd, bool reenablePlayerAtEnd)
    {
        if (blendRoutine != null)
        {
            StopCoroutine(blendRoutine);
            blendRoutine = null;
            isBlendingToNormal = false;
        }

        blendRoutine = StartCoroutine(BlendToNormalCoroutine(duration, reenableCameraAtEnd, reenablePlayerAtEnd));
    }

    private IEnumerator BlendToNormalCoroutine(float duration, bool reenableInputAtEnd, bool reenablePlayerAtEnd)
    {
        if (target == null)
        {
            yield break;
        }

        Debug.Log("Starting blend to normal follow position/rotation.");

        isBlendingToNormal = true;

        // capture start transform
        transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);

        // compute end transform at the start of the blend (stable target)
        Vector3 endPos = target.position + currentOffset;
        Vector3 lookAtPos = target.position + currentLookAtOffset;
        Quaternion endRot;
        Vector3 dir = lookAtPos - endPos;
        if (dir.sqrMagnitude > 0.000001f)
        {
            endRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
        else
        {
            endRot = transform.rotation;
        }

        float elapsed = 0f;
        // avoid divide by zero
        float dur = Mathf.Max(0.0001f, duration);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / dur));

            // Interpolate position and rotation
            transform.SetPositionAndRotation(Vector3.Lerp(startPos, endPos, t) + shakeOffset,
                                             Quaternion.Slerp(startRot, endRot, t));
            yield return null;
        }

        // ensure final values applied
        transform.SetPositionAndRotation(endPos + shakeOffset, endRot);
        isBlendingToNormal = false;
        blendRoutine = null;

        // clear suppression so normal follow logic resumes
        suppressFollow = false;

        if (reenableInputAtEnd)
        {
            SetCameraInputEnabled(true);
        }
        if (reenablePlayerAtEnd && target.TryGetComponent<PlayerController>(out var player))
        {
            player.EnableInput();
        }
    }

    // Public function to be called when a dialogue trigger is activated to start the camera zoom effect
    public void TriggerDialogueCamera(Transform dialogueTrigger, Transform secondTransform = null)
    {
        if (!isZooming)
        {
            if (secondTransform == null)
                StartCoroutine(StartCameraZoom(dialogueTrigger, true));
            else
                StartCoroutine(StartCameraZoomMidpoint(dialogueTrigger, secondTransform));
        }
    }

    // Ends the camera zoom effect and releases control back to the normal camera movement
    public IEnumerator EndCameraZoom()
    {
        if (camPosCache == Vector3.zero || camRotCache == Quaternion.identity || lookAtCache == Vector3.zero)
        {
            yield break;
        }

        // Stop any active zoom state
        isZooming = false;
        CameraLocked = false;

        camPosCache = Vector3.zero;
        camRotCache = Quaternion.identity;
        lookAtCache = Vector3.zero;

        // Smoothly blend back to normal
        StartBlendToNormal(returnBlendDuration, reenableCameraAtEnd: true, reenablePlayerAtEnd: true);

        yield break;
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

    // move camera to the midpoint between two transforms and look at that midpoint
    public IEnumerator StartCameraZoomMidpoint(Transform transformA, Transform transformB)
    {
        CameraLocked = true;
        isZooming = true;

        // Cache current camera transform
        camPosCache = transform.position;
        camRotCache = transform.rotation;

        // Midpoint between the two targets
        Vector3 midpoint = (transformA.position + transformB.position) * 0.5f;

        // Separation and a sensible distance for the camera
        float separation = Vector3.Distance(transformA.position, transformB.position);
        float distance = Mathf.Clamp(separation * 1.2f, 3f, 12f);

        // Compute a horizontal baseline between the two targets
        Vector3 baseline = transformB.position - transformA.position;
        Vector3 dir = Vector3.ProjectOnPlane(baseline, Vector3.up).normalized;

        // Build a perpendicular direction on the horizontal plane so the camera sits between the two targets
        Vector3 perpendicularDirection = Vector3.Cross(dir, Vector3.up).normalized; // perpendicular to baseline

        // choose side so camera faces the participants from the same side as the player (keeps framing natural)
        float sideSign = Mathf.Sign(Vector3.Dot(perpendicularDirection, target.position - midpoint));
        if (Mathf.Approximately(sideSign, 0f)) sideSign = 1f;

        // Choose an offset that sits perpendicular to the baseline (between them) and slightly above midpoint.
        float upFactor = Mathf.Clamp(distance * 0.12f, 0.5f, 1.5f);
        Vector3 offset = distance * sideSign * perpendicularDirection + Vector3.up * upFactor;

        // Compute average height of the two targets and aim the camera to look at that height (straight on)
        float avgHeight = (transformA.position.y + transformB.position.y) * 0.5f;

        // Target position and rotation (ensure camera Y is near avgHeight + small lift)
        Vector3 targetPos = midpoint + offset;
        targetPos.y = avgHeight + upFactor * 0.25f; // position slightly above the avg height

        Vector3 lookAtPoint = new Vector3(midpoint.x, avgHeight, midpoint.z);
        Quaternion targetRot = Quaternion.LookRotation(lookAtPoint - targetPos);

        lookAtCache = lookAtPoint;

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

        currentLookAtOffset = rotation * defaultLookAtOffset;
        
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

        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, initialRotation * defaultLookAtOffset, returnSpeed * Time.deltaTime);

        
    }

    // Predict whether moving the camera by 'delta' (world-space) would cause an overlap with environment colliders
    private bool CanMoveBy(Vector3 delta)
    {
        if (!TryGetComponent<Collider>(out var col)) return true;

        // compute the target bounds after moving camera by delta
        Bounds b = col.bounds;
        Vector3 targetCenter = b.center + delta;
        Vector3 extents = b.extents;
        Quaternion rotation = transform.rotation;

        // Query for overlapping colliders at the target location
        Collider[] hits = Physics.OverlapBox(targetCenter, extents, rotation, ~ignoreCollisionLayer, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (hit == col) return true; // ignore self

            // ignore any colliders that belong to the camera
            if (hit.transform.IsChildOf(transform)) return true;

            // Any other hit means we'd clip into something
            Debug.Log($"CanMoveBy: Camera movement blocked by {hit.name}");

            return false;
        }

        return true;
    }

    // Setter function for other scripts to lock the camera
    public void SetCameraLocked(bool locked)
    {
        CameraLocked = locked;
    }

    public void OverrideCameraPosition(WorldDirection worldDirection, Vector3 offset, Vector3 lookAtOffset, float transitionDuration = 1f)
    {
        if (!positionOverridden)
        {
            cachedOffset = defaultOffset;
            cachedLookAtOffset = defaultLookAtOffset;

            positionOverridden = true;
        }

        StartCoroutine(OverrideCameraPositionCoroutine(worldDirection, offset, lookAtOffset, transitionDuration));
    }

    private IEnumerator OverrideCameraPositionCoroutine(WorldDirection worldDirection, Vector3 overrideOffset, Vector3 overrideLookAtOffset, float transitionDuration)
    {
        Vector3 startOffset = defaultOffset;
        Vector3 startLookAtOffset = defaultLookAtOffset;

        Vector3 facingVector = DirectionToVector(worldDirection);
        Quaternion rotation = Quaternion.LookRotation(facingVector, Vector3.up);

        Vector3 targetOffset = rotation * overrideOffset;
        Vector3 targetLookAtOffset = rotation * overrideLookAtOffset;

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            defaultOffset = Vector3.Lerp(startOffset, targetOffset, t / transitionDuration);
            defaultLookAtOffset = Vector3.Lerp(startLookAtOffset, targetLookAtOffset, t / transitionDuration);
            yield return null;
        }

        defaultOffset = targetOffset;
        defaultLookAtOffset = targetLookAtOffset;
    }

    public void ResetCameraPosition()
    {
        StartCoroutine(ResetCameraPositionCoroutine(1f));

        if (positionOverridden)
        {
            positionOverridden = false;
        }
    }

    private IEnumerator ResetCameraPositionCoroutine(float transitionDuration)
    {
        Vector3 startOffset = defaultOffset;
        Vector3 startLookAtOffset = defaultLookAtOffset;

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            defaultOffset = Vector3.Lerp(startOffset, cachedOffset, t / transitionDuration);
            defaultLookAtOffset = Vector3.Lerp(startLookAtOffset, cachedLookAtOffset, t / transitionDuration);
            yield return null;
        }

        defaultOffset = cachedOffset;
        defaultLookAtOffset = cachedLookAtOffset;
    }

    public void OverrideFollowSettings(bool followPlayer = true, float smoothSpeed = -1.0f)
    {
        if (!followOverridden)
        {
            cachedFollowPlayer = this.followPlayer;
            cachedSmoothSpeed = this.smoothSpeed;

            followOverridden = true;
        }

        if (followPlayer != this.followPlayer)
        {
            this.followPlayer = followPlayer;
        }

        if (smoothSpeed != this.smoothSpeed && smoothSpeed > 0f)
        {
            this.smoothSpeed = smoothSpeed;
        }
    }

    public void ResetFollowSettings()
    {
        followPlayer = cachedFollowPlayer;
        smoothSpeed = cachedSmoothSpeed;

        if (followOverridden)
        {
            followOverridden = false;
        }
    }

    public void OverrideRotationSettings(bool rotateCamera = true, float rotateSpeed = -1.0f, bool restrictYaw = false, float maxYaw = -1.0f, float maxPitch = -1.0f)
    {
        if (!rotationOverridden)
        {
            cachedRotateSpeed = this.rotateSpeed;
            cachedRotateCamera = this.rotateCamera;
            cachedRestrictYaw = this.restrictYaw;
            cachedMaxYaw = this.maxYaw;
            cachedMaxPitch = this.maxPitch;

            rotationOverridden = true;
        }

        if (rotateSpeed != this.rotateSpeed && rotateSpeed > 0f)
        {
            this.rotateSpeed = rotateSpeed;
        }

        if (rotateCamera != this.rotateCamera)
        {
            this.rotateCamera = rotateCamera;
        }

        if (restrictYaw != this.restrictYaw)
        {
            this.restrictYaw = restrictYaw;
        }

        if (maxYaw != this.maxYaw && maxYaw > 0f)
        {
            this.maxYaw = maxYaw;
        }

        if (maxPitch != this.maxPitch && maxPitch > 0f)
        {
            this.maxPitch = maxPitch;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        playerController = newTarget.GetComponent<PlayerController>();
        pc = playerController;
        toggleInventoryUI = newTarget.GetComponent<ToggleInventoryUI>();
    }

    public void ResetRotationSettings()
    {
        rotateCamera = cachedRotateCamera;
        rotateSpeed = cachedRotateSpeed;
        restrictYaw = cachedRestrictYaw;
        maxYaw = cachedMaxYaw;
        maxPitch = cachedMaxPitch;

        if (rotationOverridden)
        {
            rotationOverridden = false;
        }
    }
}
