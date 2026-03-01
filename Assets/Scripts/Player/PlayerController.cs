using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, ISaveable
{   
    [HideInInspector] public CharacterController Controller;
    private Camera PlayerCamera;
    [HideInInspector] public Animator Animator;

    [Header("Sprint UI Colors")]
    [SerializeField] Color normalColor = new Color(0f, 147f/255f, 111f/255f);
    [SerializeField] Color cooldownColor = Color.grey;
    [SerializeField] ParticleSystem SprintDust;

    [Header("Sprint Fade Settings")]
    private Coroutine staminaFadeRoutine;
    public float staminaFadeDuration = 1.5f;
    private CanvasGroup staminaGroup;

    [Header("Movement Settings")]
    public bool MovementLocked = false;
    public float Speed = 1f;
    public float SprintSpeed = 2f;
    public float SprintDuration = 3f;
    public float sprintCooldown = 4f;
    [SerializeField, Tooltip("The speed the player moves at when their sprint stamina is at 0. Only relevant for when they're holding a moveable object. Lower numbers = lower speed")] 
    private float emptyStaminaSpeedFactor = 0.5f;
    [SerializeField] private bool StationaryCamera;

    public float SprintTimer = 3f;
    private bool canSprint = true;
    private bool sprintOnCooldown = false;
    private bool isSprinting = false;
    private PlayerMovingObjects mover;
    private (bool movingObject, float sprintDepletionRate, float staminaDecay, bool allowSprint) moveableObjectMod = (false, 1f, 1f, true);
    private Coroutine sprintCooldownRoutine;

    // Gravity / Ground settings
    private float yVelocity = 0f;
    private readonly float gravity = -9.81f;
    private bool gravityEnabled = true;
    private bool freezePosition = false;

    [Header("Ground check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundMask;
    private bool isGrounded;

    public static bool DialogueActive = false;

    // Special Idle Variables
    private float idleTimer = 0;
    private bool specialIdle = false;

    // Input System
    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 lookInput;
    private readonly float deadzone = 0.15f;
    private bool cutsceneLocked = false;
    private Vector3 lockedPosition;
    public bool showDebugLogs = false;
    private readonly bool resetLocked = false;
    private PlayerThrowing playerThrowing;
    private bool isThrowing;
    private bool isMoving;
    private readonly float moveCheckDelay = 0.1f;
    private float lastStoppedCheck = -1f;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        Animator = GetComponentInChildren<Animator>();

        if (GameManager.Instance.staminaSlider != null)
        {
            GameManager.Instance.staminaSlider.maxValue = SprintDuration;
        }

        staminaGroup = GameManager.Instance.staminaSlider.GetComponent<CanvasGroup>();
        if (staminaGroup != null)
        {
            staminaGroup.alpha = 1f;
        }

        if (PlayerCamera == null)
            PlayerCamera = Camera.main;

        SprintTimer = SprintDuration;

        if (GameManager.Instance.staminaSlider != null)
        {
            GameManager.Instance.staminaSlider.value = SprintTimer;
            GameManager.Instance.staminaSlider.gameObject.SetActive(false);
        }

        playerThrowing = gameObject.GetComponent<PlayerThrowing>();

        // Create ground check if missing
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            groundCheck = groundCheckObj.transform;
        }

        // Initialize input actions
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = ctx.ReadValue<Vector2>();

        controls.Player.Sprint.performed += ctx => StartSprinting();
        controls.Player.Sprint.canceled += ctx => StopSprinting();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();

        rb = GetComponent<Rigidbody>();
        mover = GetComponent<PlayerMovingObjects>();
    }

    public void SaveTo(SaveData data)
    {
        float[] position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        float[] rotation = new float[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        data.playerSaveData.SetPlayerTransform(SceneManager.GetActiveScene().name, position, rotation);
        if (TimerRingUI.Instance != null)
        {
            data.playerSaveData.currentRingState = TimerRingUI.Instance.currentRingState;
        }
        else
        {
            Debug.Log("TimerRingUI.Instance == null! cannot save current ring state");
        }
        
    }

    public void LoadFrom(SaveData data)
    {
        if (data.playerSaveData.TryGetPlayerTransform(SceneManager.GetActiveScene().name, out float[] position, out float[] rotation))
        {
            transform.position = new Vector3(position[0], position[1], position[2]);
            transform.eulerAngles = new Vector3(rotation[0], rotation[1], rotation[2]);
            Debug.Log("Player transform loaded for scene: " + SceneManager.GetActiveScene().name + " Position: " + transform.position + " Rotation: " + transform.eulerAngles);
        }
        else
        {
            Debug.LogWarning("No saved transform found for player in scene: " + SceneManager.GetActiveScene().name);
        }

        if (TimerRingUI.Instance != null && data.playerSaveData.currentRingState != TimerRingUI.RingState.Empty)
        {
            TimerRingUI.Instance.SetRingState(data.playerSaveData.currentRingState);
            // GameOverManager.Instance.
        }
        else if (TimerRingUI.Instance != null)
        {
            TimerRingUI.Instance.SetRingState(TimerRingUI.RingState.Full);
        }
        
    }

    private void OnEnable() => controls?.Enable();
    private void OnDisable() => controls?.Disable();

    private void Update()
    {
        if (DialogueActive)
        {
            moveInput = Vector2.zero;
            yVelocity = 0f;
            return;
        }

        if (moveInput.sqrMagnitude < deadzone)
        {
            moveInput = Vector2.zero;
        }

        Movement();

        if (moveInput != Vector2.zero && showDebugLogs)
        {
            Debug.Log("MOVE INPUT: " + moveInput);
        }

        if (playerThrowing != null)
        {
            isThrowing = playerThrowing.GetIsCharging();
        }
    }

    private void Movement()
    {
        if (cutsceneLocked)
        {
            transform.position = lockedPosition;
            return;
        }

        if (resetLocked)
        {
            Controller.Move(Vector3.zero);
            return;
        }
        else
        {
            Controller.enabled = true;
        }
        
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (isGrounded && yVelocity < 0f)
            yVelocity = -1f;

        if (freezePosition)
        {
            if (gravityEnabled)
            {
                yVelocity += gravity * Time.deltaTime;
            }

            Controller.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
            return;
        }

        if (MovementLocked)
        {
            ResetAnimations();
            Animator.SetBool("isIdle", true);
            moveInput = Vector2.zero;
            if (gravityEnabled)
            {
                yVelocity += gravity * Time.deltaTime;
            }

            Controller.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
            return;
        }

        // Calculate movement direction
        Vector3 move = Vector3.zero;
        if (PlayerCamera != null)
        {
            Vector3 camForward = PlayerCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = PlayerCamera.transform.right;
            camRight.y = 0f;
            camRight.Normalize();
            move = camForward * moveInput.y + camRight * moveInput.x;
            
        }
        else
        {
            PlayerCamera = Camera.main;
        }

        float currentSpeed = Speed;

        // Only set isMoving to false after no movement is detected for a short period
        if (moveInput != Vector2.zero)
        {
            isMoving = true;
            lastStoppedCheck = -1f;
        }
        else if (lastStoppedCheck <= 0f)
        {
            // Mark the time when we first detect no movement input
            lastStoppedCheck = Time.time;
        }
        else if (Time.time > (lastStoppedCheck + moveCheckDelay))
        {
            // Enough time has passed for the player to be considered stopped
            isMoving = false;
            lastStoppedCheck = -1f;
        }

        //gets isMoving state to change animator state to idle or walking
        if (isMoving)
        {
            ResetAnimations();
            Animator.SetBool("isWalking", true);
            idleTimer = 0f; // reset idle timer
        }
        if (!isMoving)
        {
            if (!specialIdle)
            {
                ResetAnimations();
                Animator.SetBool("isIdle", true);
            }

            idleTimer += Time.deltaTime;

            if (!specialIdle && idleTimer >= 15f)
            {
                idleTimer = 0f;
                StartCoroutine(PlaySpecialIdle());
            }
        }

        if (!sprintOnCooldown)
        {
            // Only change canSprint if cooldown isn't active to avoid unintended re-enabling of sprint
            canSprint = moveableObjectMod.allowSprint;
        }

        if (GameManager.Instance.staminaFill != null && GameManager.Instance.staminaSlider != null)
        {
            // Sprint logic
            if (isSprinting && canSprint && !sprintOnCooldown)
            {
    
                if (isMoving && SprintTimer > 0f)
                {
                    currentSpeed = SprintSpeed;
                    float depletionRate = moveableObjectMod.movingObject ? moveableObjectMod.sprintDepletionRate : 1f;
                    SprintTimer -= Time.deltaTime * depletionRate; // Stamina depletes faster if moving an object
                    if (staminaGroup != null)
                    {
                        staminaGroup.alpha = 1f;
                    }
                    GameManager.Instance.staminaSlider.gameObject.SetActive(true);
                    GameManager.Instance.staminaSlider.value = SprintTimer; //sets slider to stamina when going down
                    Animator.speed = 1.4f; // speeds up walk animation when sprinting

                    if (showDebugLogs)
                        Debug.Log("Sprinting. Current SprintTimer: " + SprintTimer);
                }
                else if (!moveableObjectMod.movingObject)
                {
                    canSprint = false;
                    isSprinting = false;
                    currentSpeed = Speed;

                    if (GameManager.Instance.staminaFill != null)
                        GameManager.Instance.staminaFill.color = cooldownColor;

                    if (sprintCooldownRoutine != null)
                    {
                        StopCoroutine(sprintCooldownRoutine);
                    }
                    sprintOnCooldown = true;
                    sprintCooldownRoutine = StartCoroutine(SprintCooldown());
                }
            }
            else if (moveableObjectMod.movingObject && !isSprinting && isMoving && SprintTimer > 0f)
            {
                // Decay sprint stamina when walking with an object in hand
                if (sprintCooldownRoutine != null)
                {
                    // Stop any existing cooldown routine if we start moving an object again to prevent premature reset of SprintTimer
                    StopCoroutine(sprintCooldownRoutine);
                    sprintCooldownRoutine = null;
                }
                SprintTimer -= Time.deltaTime * moveableObjectMod.staminaDecay;
                if (staminaGroup != null)
                {
                    staminaGroup.alpha = 1f;
                }
                GameManager.Instance.staminaSlider.gameObject.SetActive(true);
                GameManager.Instance.staminaSlider.value = SprintTimer;
                // Don't start cooldown to ensure SprintTimer doesn't reset prematurely

                if (showDebugLogs)
                    Debug.Log("Depleting stamina while moving object. Current SprintTimer: " + SprintTimer);
            }
            else if (!moveableObjectMod.movingObject && !sprintOnCooldown && !isSprinting && SprintTimer <= 0f)
            {
                // If not moving an object, is not sprinting and stamina is fully depleted, start cooldown
                isSprinting = false;
                Animator.SetBool("isSprinting", false);
            }

            //Out of Stamina
            if (moveableObjectMod.movingObject && SprintTimer <= 0f)
            {
                // When stamina is empty and moving an object, slow down movement even further
                currentSpeed = Speed * emptyStaminaSpeedFactor;

                isSprinting = false;
                canSprint = false;
            }
            else if (SprintTimer <= 0f && !sprintOnCooldown)
            {
                isSprinting = false;
                Animator.SetBool("isSprinting", false);

                canSprint = false;
                if (GameManager.Instance.staminaFill != null)
                    GameManager.Instance.staminaFill.color = cooldownColor;

                if (sprintCooldownRoutine != null)
                {
                    StopCoroutine(sprintCooldownRoutine);
                }
                sprintOnCooldown = true;
                sprintCooldownRoutine = StartCoroutine(SprintCooldown());
            }
            else if (!moveableObjectMod.movingObject && !isSprinting && SprintTimer < SprintDuration)
            {
                // Regenerating stamina
                Animator.SetBool("isSprinting", false);
                SprintTimer += Time.deltaTime;
                if (staminaGroup != null)
                {
                    staminaGroup.alpha = 1f;
                }
                GameManager.Instance.staminaSlider.gameObject.SetActive(true);
                GameManager.Instance.staminaSlider.value = SprintTimer;

                if (showDebugLogs)
                    Debug.Log("Regenerating stamina. Current SprintTimer: " + SprintTimer);
            }
            else if (!isSprinting && SprintTimer >= SprintDuration)
            {
                // Stamina is full, reset values
                SprintTimer = SprintDuration;

                 if (staminaFadeRoutine != null)
                {
                    StopCoroutine(staminaFadeRoutine);
                }
                staminaFadeRoutine = StartCoroutine(StaminaFadeAway());

                GameManager.Instance.staminaSlider.value = SprintTimer;
                canSprint = true;
                sprintOnCooldown = false;
                if (GameManager.Instance.staminaFill != null)
                {
                    GameManager.Instance.staminaFill.color = normalColor;
                }

                if (GameManager.Instance.staminaSlider.value >= GameManager.Instance.staminaSlider.maxValue)
                {
                    //if (staminaSlider.gameObject.activeSelf) Debug.Log("Stamina full, deactivating stamina slider.");

                    if (sprintCooldownRoutine != null)
                    {
                        StopCoroutine(sprintCooldownRoutine); // Ensure cooldown is stopped when stamina is full
                        sprintCooldownRoutine = null;
                    }

                    GameManager.Instance.staminaSlider.gameObject.SetActive(false);
                }
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("Stamina slider or fill not assigned in PlayerController.");
        }

        if (gravityEnabled)
            yVelocity += gravity * Time.deltaTime;

        // Build movement vectors
        Vector3 horizontalMove = (move.normalized * currentSpeed) * Time.deltaTime;
        Vector3 verticalMove = new Vector3(0f, yVelocity, 0f) * Time.deltaTime;

        // If player is holding an object, ask PlayerMovingObjects if the horizontal move would cause clipping.
        if (moveableObjectMod.movingObject)
        {
            bool canMove = mover.CanMoveBy(horizontalMove);
            if (!canMove)
            {
                // block horizontal movement while still allowing vertical (gravity) to apply
                horizontalMove = Vector3.zero;
                // also mark not moving to avoid sprint/stamina drain visual change
                isMoving = false;
            }
        }

        Vector3 combined = horizontalMove + verticalMove;
        Controller.Move(combined);

        if (move.sqrMagnitude > 0.01f && !isThrowing)
        {
            float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else if(isThrowing && !StationaryCamera)
        {
            Vector3 camForward = PlayerCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            float angle = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else if(isThrowing && StationaryCamera)
        {
            if(lookInput.magnitude > 0)
            {
                float angle = Mathf.Atan2(lookInput.x, -lookInput.y) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, -angle, 0f);
            }
        }
    }

    public void SetDialogueActive(bool active)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning("Rigidbody component not found on PlayerController.");
                return;
            }
        }

        ResetAnimations();
        Animator.SetBool("isIdle", true);
        DialogueActive = active;
        if (active == true)
        {
            moveInput = Vector2.zero; // stop leftover movement
            Controller.Move(Vector3.zero); // ensure no residual motion

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            
            freezePosition = true;
        }
        else
        {
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }

            freezePosition = false;

            yVelocity = 0f;
        }
    }

    private void StartSprinting() //also handles animations
    {
        bool isMoving = moveInput.sqrMagnitude >= 0.01f;
        if (!isSprinting)
        {
            ResetAnimations();
        }
        if (canSprint && isMoving)
        {
            isSprinting = true;
            Animator.SetBool("isSprinting", true);

            if (SprintDust != null && !SprintDust.isPlaying)
            {
                //Debug.Log("Started Dust");
                SprintDust.Play();
            }
        }
    }

    private void StopSprinting()
    {
        if (isSprinting)
        {
            ResetAnimations();
        }
        isSprinting = false;
        Animator.SetBool("isSprinting", false);

         if (staminaGroup != null)
        {
            staminaGroup.alpha = 1f;
        }

        GameManager.Instance.staminaSlider.gameObject.SetActive(true);

        if (staminaFadeRoutine != null)
        {
            StopCoroutine(staminaFadeRoutine);
            staminaFadeRoutine = null;
        }

        if (SprintDust != null && SprintDust.isPlaying)
        {
            //Debug.Log("Stopped Dust");
            SprintDust.Stop();
        }
    }

    IEnumerator SprintCooldown()
    {
        if (showDebugLogs)
            Debug.Log("Sprint cooldown started.");
        yield return new WaitForSeconds(sprintCooldown);
        if (showDebugLogs)
            Debug.Log("Sprint cooldown ended. Stamina reset.");
        SprintTimer = SprintDuration;
        canSprint = true;
        sprintOnCooldown = false;

        if (GameManager.Instance.staminaFill != null)
        {
            GameManager.Instance.staminaFill.color = normalColor;
        }

        sprintCooldownRoutine = null;
    }

    public void MovingObject(bool isMovingObject, float sprintReduction = 1f, float staminaDecay = 1f, bool allowSprint = true)
    {
        moveableObjectMod = (isMovingObject, sprintReduction, staminaDecay, allowSprint);
        if (!sprintOnCooldown && allowSprint == false)
        {
            // If not currently in cooldown and sprint is disallowed, change canSprint directly
            canSprint = allowSprint;
        }

        if (showDebugLogs)
            Debug.Log($"MovingObject called. isMovingObject: {isMovingObject}, sprintReduction: {sprintReduction}, staminaDecay: {staminaDecay}, allowSprint: {allowSprint}");
    }

    public void SetVerticalVelocity(float newVelocity) => yVelocity = newVelocity;
    public float GetVerticalVelocity() => yVelocity;
    public void AddVerticalVelocity(float delta) => yVelocity += delta;
    public void SetGravityEnabled(bool enabled) => gravityEnabled = enabled;
    public void SetFreezePosition(bool freeze)
    {
        freezePosition = freeze;
        if (freeze) yVelocity = 0f;
    }
    public void SetCanSprint(bool newCanSprint) => canSprint = newCanSprint;

    public void SetCutsceneLocked(bool locked)
    {
        ResetAnimations(); //resets animation to idle during a cutscene
        Animator.SetBool("isIdle", true);
        cutsceneLocked = locked;

        if (locked)
        {
            lockedPosition = transform.position;
            yVelocity = 0f;
            moveInput = Vector2.zero;
            isSprinting = false;
            canSprint = false;
        }
        else
        {
            Controller.enabled = false;
            yVelocity = -1f;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + 0.1f,
                transform.position.z
            );
            Controller.enabled = true;
            canSprint = true;
        }
    }

    public void SetResetLock(bool locked)
    {
        if (locked)
        {
            yVelocity = 0f;
            moveInput = Vector2.zero;
            isSprinting = false;
            canSprint = false;
        }
        else
        {
            canSprint = true;
        }
    }

    public void TeleportTo(Vector3 newPosition, Quaternion newRotation)
    {
        Controller.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        transform.SetPositionAndRotation(newPosition, newRotation);
        Controller.enabled = true;

    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private IEnumerator PlaySpecialIdle()
    {
        specialIdle = true;

        ResetAnimations();
        Animator.SetTrigger("specialIdle");

        yield return new WaitForSeconds(2f);

        specialIdle = false;
    }

    public IEnumerator CollectAnimationDelay()
    {
        if (!Animator)
        {
            Debug.LogWarning("Animator not found on the player in the scene");
            yield break;
        }

        Animator.SetBool("isCollecting", true);
        Animator.SetTrigger("collect");
        DisableInput();
        yield return new WaitForSeconds(1.5f);
        Animator.SetBool("isCollecting", false);
        EnableInput();
    }

    private IEnumerator StaminaFadeAway() //Fades the stamina bar away slowly
    {
        if (staminaGroup == null) yield break;

        float t = 0f; //time float
        float start = staminaGroup.alpha;

        while (t < staminaFadeDuration) 
        {
            t += Time.deltaTime;
            staminaGroup.alpha = Mathf.MoveTowards(start, 0f, t * staminaFadeDuration);

            if (staminaGroup.alpha <= 0.01f) //instantly dissapears the bar at lower floats to avoid very low numbers like 0.000019764
            {
                staminaGroup.alpha = 0f;
                break;
            }
            yield return null;
        }

        staminaGroup.alpha = 0f;
        GameManager.Instance.staminaSlider.gameObject.SetActive(false);
    }

    public void DisableInput() // for disabling/freezing the player throughout other scripts
    {
        controls.Disable();
    }

    public void EnableInput() // for enabling/unfreezing the player throughout other scripts
    {
        controls.Enable();
    }

    private void ResetAnimations()
    {
        Animator.SetBool("isIdle", false);
        Animator.SetBool("isWalking", false);
        //Animator.SetBool("isGrabbing", false);
        Animator.SetBool("isFloating", false);
        //Animator.SetBool("isCollecting", false);
    }
}
