using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, ISaveable
{   
    [HideInInspector] public CharacterController Controller;
    private Camera PlayerCamera;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Slider staminaSlider;
    [HideInInspector] public Image staminaFill;

    [Header("Sprint UI Colors")]
    public Color normalColor = new Color(0f, 147f/255f, 111f/255f);
    public Color cooldownColor = Color.grey;

    [Header("Movement Settings")]
    public bool MovementLocked = false;
    public float Speed = 1f;
    public float SprintSpeed = 2f;
    public float SprintDuration = 3f;
    public float sprintCooldown = 4f;
    public float staminaLingerDuration = 1f;
    private float staminaLingerTimer = 0f;

    public float SprintTimer = 3f;
    private bool canSprint = true;
    private bool sprintOnCooldown = false;
    private bool isSprinting = false;
    private (bool movingObject, float sprintDepletionRate, float sprintDecay, bool allowSprint) moveableObjectMod = (false, 1f, 1f, true);
    private Coroutine sprintCooldownRoutine;

    [Header("Gravity / Ground Settings")]
    private float yVelocity = 0f;
    private float gravity = -9.81f;
    private bool gravityEnabled = true;
    private bool freezePosition = false;

    [Header("Ground check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;
    private bool isGrounded;

    public static bool DialogueActive = false;

    [Header("Special Idle Parameters")]
    public float idleTimer = 0;
    private bool specialIdle = false;


    // Input System
    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 moveInput;
    private float deadzone = 0.15f;
    private bool cutsceneLocked = false;
    private Vector3 lockedPosition;
    public bool showDebugLogs = false;
    private bool resetLocked = false;
    private PlayerThrowing playerThrowing;
    private bool isThrowing;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();


        if (staminaSlider == null)
        {
            staminaSlider = GameObject.Find("StaminaSlider")?.GetComponent<Slider>();
        }

        if (staminaFill == null)
        {
            staminaFill = GameObject.Find("StaminaFill")?.GetComponent<Image>();
        }

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = SprintDuration;
        }

        if (PlayerCamera == null)
            PlayerCamera = Camera.main;

        SprintTimer = SprintDuration;

        if (staminaSlider != null)
        {
            staminaSlider.value = SprintTimer;
            staminaSlider.gameObject.SetActive(false);
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
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Sprint.performed += ctx => StartSprinting();
        controls.Player.Sprint.canceled += ctx => StopSprinting();

        rb = GetComponent<Rigidbody>();

       // controls.Player.LoadArtScene.performed += ctx => LoadArtScene();
       // controls.Player.LoadMenuScene.performed += ctx => LoadMenuScene();
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

        bool isMoving = move.sqrMagnitude >= 0.01f; //detects if the player is moving

        //animator.SetBool("isWalking", isMoving);
        //animator.SetBool("isIdle", !isMoving);

        //gets isMoving state to change animator state to idle or walking
        if (isMoving)
        {
            resetAnimations();
            animator.SetBool("isWalking", true);
            idleTimer = 0f; // reset idle timer
        }
        if (!isMoving)
        {
            if (!specialIdle)
            {
                resetAnimations();
                animator.SetBool("isIdle", true);
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
            // Change canSprint if cooldown isn't active to avoid unintended re-enabling of sprint
            canSprint = moveableObjectMod.allowSprint;
        }

        if (staminaFill != null && staminaSlider != null)
        {
            // Sprint logic
            if (isSprinting && canSprint && !sprintOnCooldown)
            {
    
            if (isMoving && SprintTimer > 0f)
                {
                    currentSpeed = SprintSpeed;
                    float depletionRate = moveableObjectMod.movingObject ? moveableObjectMod.sprintDepletionRate : 1f;
                    SprintTimer -= Time.deltaTime * depletionRate; // Stamina depletes faster if moving an object
                    staminaSlider.gameObject.SetActive(true);
                    staminaSlider.value = SprintTimer; //sets slider to stamina when going down
                    animator.speed = 1.4f; // speeds up walk animation when sprinting

                    //Debug.Log("Sprinting. Current SprintTimer: " + SprintTimer);
                }
                else if (!moveableObjectMod.movingObject)
                {
                    canSprint = false;
                    isSprinting = false;
                    currentSpeed = Speed;

                    if (staminaFill != null)
                        staminaFill.color = cooldownColor;

                    if (sprintCooldownRoutine != null)
                    {
                        StopCoroutine(sprintCooldownRoutine);
                    }
                    sprintOnCooldown = true;
                    sprintCooldownRoutine = StartCoroutine(SprintCooldown());
                }
            }
            else if (moveableObjectMod.movingObject && !isSprinting && SprintTimer > 0f)
            {
                // Decay sprint stamina when carrying an object
                if (sprintCooldownRoutine != null)
                {
                    // Stop any existing cooldown routine if we start moving an object again to prevent premature reset of SprintTimer
                    StopCoroutine(sprintCooldownRoutine);
                    sprintCooldownRoutine = null;
                }
                SprintTimer -= Time.deltaTime * moveableObjectMod.sprintDecay;
                staminaSlider.gameObject.SetActive(true);
                staminaSlider.value = SprintTimer;
                // Don't start cooldown to ensure SprintTimer doesn't reset prematurely

                //Debug.Log("Depleting stamina while moving object. Current SprintTimer: " + SprintTimer);
            }
            else if (!moveableObjectMod.movingObject && !sprintOnCooldown && !isSprinting && SprintTimer < Mathf.Epsilon)
            {
                // If not moving an object, is not sprinting and stamina is fully depleted, start cooldown
                isSprinting = false;
                animator.SetBool("isSprinting", false);
            }

            //Out of Stamina
            if (SprintTimer <= 0f && !sprintOnCooldown)
            {
                isSprinting = false;
                animator.SetBool("isSprinting", false);

                canSprint = false;
                if (staminaFill != null)
                    staminaFill.color = cooldownColor;

                if (sprintCooldownRoutine != null)
                {
                    StopCoroutine(sprintCooldownRoutine);
                }
                sprintOnCooldown = true;
                sprintCooldownRoutine = StartCoroutine(SprintCooldown());
            }
            else if (!moveableObjectMod.movingObject && !isSprinting && SprintTimer < SprintDuration )
            {
                // Regenerating stamina
                animator.SetBool("isSprinting", false);
                SprintTimer += Time.deltaTime;
                staminaSlider.gameObject.SetActive(true);
                staminaSlider.value = SprintTimer;

                //Debug.Log("Regenerating stamina. Current SprintTimer: " + SprintTimer);
            }
            else if (!isSprinting && SprintTimer >= SprintDuration)
            {
                // Stamina is full, reset values
                SprintTimer = SprintDuration;
                staminaSlider.value = SprintTimer;
                canSprint = true;
                sprintOnCooldown = false;
                if (staminaFill != null)
                {
                    staminaFill.color = normalColor;
                }

                if (staminaSlider.value >= staminaSlider.maxValue)
                {
                    //if (staminaSlider.gameObject.activeSelf) Debug.Log("Stamina full, deactivating stamina slider.");

                    if (sprintCooldownRoutine != null)
                    {
                        StopCoroutine(sprintCooldownRoutine); // Ensure cooldown is stopped when stamina is full
                        sprintCooldownRoutine = null;
                    }

                    staminaSlider.gameObject.SetActive(false);
                }

                //if (staminaLingerTimer <= 0f)
                //{
                //    staminaLingerTimer = staminaLingerDuration; // reset the linger countdown
                //}

                //staminaLingerTimer -= Time.deltaTime;

                //// When linger finishes, hide the slider
                //if (staminaLingerTimer <= 0f)
                //{
                //    staminaSlider.gameObject.SetActive(false);
                //}
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("Stamina slider or fill not assigned in PlayerController.");
        }

        if (gravityEnabled)
            yVelocity += gravity * Time.deltaTime;

        Vector3 combined = (move.normalized * currentSpeed) + new Vector3(0f, yVelocity, 0f);
        Controller.Move(combined * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f && !isThrowing)
        {
            float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else if(isThrowing)
        {
            Vector3 camForward = PlayerCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            float angle = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
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
            resetAnimations();
        }
        if (canSprint && isMoving)
        {
            isSprinting = true;
            animator.SetBool("isSprinting", true);
        }
    }

    private void StopSprinting()
    {
        if (isSprinting)
        {
            resetAnimations();
        }
        isSprinting = false;
        animator.SetBool("isSprinting", false);
    }

    IEnumerator SprintCooldown()
    {
        //Debug.Log("Sprint cooldown started.");
        yield return new WaitForSeconds(sprintCooldown);
        //Debug.Log("Sprint cooldown ended. Stamina reset.");
        SprintTimer = SprintDuration;
        canSprint = true;
        sprintOnCooldown = false;

        if (staminaFill != null)
        {
            staminaFill.color = normalColor;
        }

        sprintCooldownRoutine = null;
    }

    public void MovingObject(bool isMovingObject, float sprintReduction = 1f, float sprintDecay = 1f, bool allowSprint = true)
    {
        moveableObjectMod = (isMovingObject, sprintReduction, sprintDecay, allowSprint);
        if (!sprintOnCooldown && allowSprint == false)
        {
            // If not currently in cooldown and sprint is disallowed, change canSprint directly
            canSprint = allowSprint;
        }

        //Debug.Log($"MovingObject called. isMovingObject: {isMovingObject}, sprintReduction: {sprintReduction}, sprintDecay: {sprintDecay}, allowSprint: {allowSprint}");
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

    private void LoadArtScene()
    {
        SceneManager.LoadScene("ArtScene");
    }

    private void LoadMenuScene()
    {
        SceneManager.LoadScene("MenuTesting");
    }

    private IEnumerator PlaySpecialIdle()
    {
        specialIdle = true;

        resetAnimations();
        animator.SetTrigger("specialIdle");

        yield return new WaitForSeconds(2f);

        specialIdle = false;
    }


    private void resetAnimations()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        //animator.SetBool("isGrabbing", false);
        animator.SetBool("isFloating", false);
    }
}
