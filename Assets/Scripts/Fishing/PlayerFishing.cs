using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class PlayerFishing : MonoBehaviour
{
    [Header("Input")]
    [SerializeField, Tooltip("Control input for casting/reeling")] 
    private InputActionReference fishingAction;

    [Header("Cast / Reel")]
    [SerializeField, Tooltip("Force with which the hook is cast out")] 
    private float castForce = 10f;
    [SerializeField, Tooltip("How far from the player the cast target can move")] 
    private float maxCastDistance = 10f;
    [SerializeField, Tooltip("How close to the player the cast target can move")] 
    private float minCastDistance = 2f;
    [SerializeField, Tooltip("How quickly the line gets reeled in")] 
    private float reelSpeed = 8f;
    [SerializeField, Tooltip("Max distance from the player to auto-hold reeled-in objects")] 
    private float pickupDistance = 3.0f; // distance to auto-hold reeled object
    [SerializeField, Tooltip("Time after the hook is cleaned up until the line can be cast again")] 
    private float castCooldown = 0.5f; // time after the hook is cleaned up until it can be cast again
    [SerializeField, Tooltip("Minimum amount of time that can pass for reeling to be enabled (reeling is also enabled if the hook moves past the Min Cast Distance")] 
    private float reelEnableDelay = 0.75f;   // short delay before enabling reel
    [SerializeField] private bool disableMovementWhileFishing = true;
    [SerializeField, Tooltip("Speed at which the visible target sweeps between min and max range.")]
    private float targetSweepSpeed = 1f;

    [Header("References")]
    [SerializeField] private ItemData fishingRod;
    [SerializeField, Tooltip("Where the hook will spawn from")] private Transform castOrigin;
    [SerializeField, Tooltip("Found in the Fishing Canvas prefab")] private Slider castChargeSlider;
    [SerializeField, Tooltip("Prefab used to show the moving cast target while charging. If null, a small primitive sphere will be used.")]
    private GameObject castTargetPrefab;

    private PlayerEquipItem playerEquipItem;
    private GameObject hook; // hook object. Should be the only child of the Fishing Rod (Functional) prefab

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private LineRenderer line;
    private HookController currentHookController;
    private bool canReel = false;
    private bool canCast = true;
    private bool input = false;         // whether the cast/reel button is currently held (for reeling)
    private bool isFishing = false;     // whether the hook is currently cast/out
    private bool isCharging = false;
    private bool prevRbUseGravity;
    private bool prevRbKinematic;
    private RigidbodyConstraints prevRbConstraints;
    private bool isInCleanupState = false;
    private float castTime;
    private float chargeStartTime;
    private float fishingStartTime;

    // cast target state
    private GameObject castTargetInstance;
    private Coroutine castTargetRoutine;
    private float currentTargetDistance;
    private Vector3 pendingTargetPosition;
    private bool hasPendingTarget = false;

    private void OnEnable()
    {
        if (fishingAction != null)
        {
            fishingAction.action.started += OnInputStarted;
            fishingAction.action.canceled += OnInputCanceled;
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("PlayerFishing: Fishing input action reference is missing. Fishing controls will not work.");
        }
    }
    private void OnDisable()
    {
        fishingAction.action.started -= OnInputStarted;
        fishingAction.action.canceled -= OnInputCanceled;
        CleanupHook();
        StopAllCoroutines();
    }

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;

        if (playerEquipItem == null)
        {
            playerEquipItem = GetComponent<PlayerEquipItem>();
            if (playerEquipItem == null)
            {
                Debug.LogError("PlayerFishing: PlayerEquipItem reference is missing.");
                enabled = false;
            }
        }

        if (hook != null)
        {
            hook.SetActive(false);
        }

        // prepare charge slider UI
        if (castChargeSlider != null)
        {
            castChargeSlider.minValue = 0f;
            castChargeSlider.maxValue = 1f;
            castChargeSlider.value = 0f;
            castChargeSlider.gameObject.SetActive(false);
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("PlayerFishing: Cast charge slider reference is missing. Charge UI will not be shown.");
        }

        if (!PlayerComponents.initialized) PlayerComponents.InitializeComponents(gameObject);

    }

    private void OnInputStarted(InputAction.CallbackContext ctx)
    {
        if (showDebugLogs) Debug.Log("Fishing input started. Context: " + ctx);
        if (!HasRod())
        {
            if (showDebugLogs) Debug.LogWarning("PlayerFishing: Attempted to use fishing input without having the fishing rod equipped.");
            return;
        }

        if (hook == null)
        {
            if (HasRod())
            {
                if (showDebugLogs) Debug.Log("PlayerFishing: Attempting to find hook object as child of equipped fishing rod.");
                hook = playerEquipItem.GetEquippedItemInstance().transform.GetChild(0).gameObject;

                PlayerComponents.SetCertainComponents(false, source: gameObject, PlayerComponents.playerFloating, PlayerComponents.playerPossessing, PlayerComponents.playerMantling);
            }
            else
            {
                Debug.LogError("PlayerFishing: Couldn't find a hook");
                return;
            }
        }

        // If player is not currently casting and is allowed to cast, start charging
        if (canCast && !canReel && !isFishing && HasRod() && !hook.activeSelf)
        {
            StartCharging();
        }
        else if (canReel)
        {
            // normal reel input
            input = true;
            if (showDebugLogs) Debug.Log("Reel input started.");
        }
        else
        {
            if (showDebugLogs) Debug.Log($"Cannot start cast or reel: CanCast={canCast} IsCasting={isFishing} HasRod={HasRod()}");
        }
    }

    private void OnInputCanceled(InputAction.CallbackContext ctx)
    {
        if (showDebugLogs) Debug.Log("Fishing input canceled. Context: " + ctx);
        if (!HasRod() || hook == null)
        {
            if (showDebugLogs) Debug.LogError("PlayerFishing: Invalid state on input cancel. HasRod: " + HasRod() + ", Hook is null: " + (hook == null));
            return; 
        }

        // If we were charging, release to cast
        if (isCharging)
        {
            if (showDebugLogs) Debug.Log("Input released while charging - releasing charge to cast.");
            ReleaseCharge();
        }
        else
        {
            // stop reel input
            input = false;
            if (showDebugLogs) Debug.Log("Reel input released.");
        }
    }

    private void StartCharging()
    {
        if (showDebugLogs) Debug.Log("Started charging cast.");

        isCharging = true;
        chargeStartTime = Time.time;

        if (castChargeSlider != null)
        {
            castChargeSlider.value = 0f;
            castChargeSlider.gameObject.SetActive(true);
        }

        // spawn visible target and start sweeping between min/max
        SpawnCastTarget();
        castTargetRoutine = StartCoroutine(SweepCastTarget());
    }

    private void ReleaseCharge()
    {
        if (!isCharging) return;

        // Use the same sweep phase for captured charge value
        float phase = Mathf.PingPong((Time.time - chargeStartTime) * targetSweepSpeed, 1f);
        float normalized = Mathf.Clamp01(phase);

        // Stop target sweep and capture final target position
        if (castTargetRoutine != null)
        {
            StopCoroutine(castTargetRoutine);
            castTargetRoutine = null;
        }
        if (castTargetInstance != null)
        {
            pendingTargetPosition = castTargetInstance.transform.position;
            hasPendingTarget = true;
        }

        if (showDebugLogs) Debug.Log($"Released charge. ChargePhase: {normalized:F2} -> targetDistance={currentTargetDistance:F2}");

        isCharging = false;
        if (castChargeSlider != null)
        {
            castChargeSlider.value = 0f;
            castChargeSlider.gameObject.SetActive(false);
        }

        // hide the visible target now (StartCast will use pendingTargetPosition)
        DestroyCastTarget();

        // perform the cast and send the hook to land on the pending target
        StartCast(); // StartCast will check hasPendingTarget
    }

    private void SpawnCastTarget()
    {
        DestroyCastTarget();

        if (castTargetPrefab != null)
        {
            castTargetInstance = Instantiate(castTargetPrefab, castOrigin.position + castOrigin.forward * minCastDistance, Quaternion.identity);
        }
        else
        {
            // fallback primitive indicator
            castTargetInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (castTargetInstance.TryGetComponent<Collider>(out var col)) Destroy(col);
            castTargetInstance.transform.position = castOrigin.position + castOrigin.forward * minCastDistance;
            castTargetInstance.hideFlags = HideFlags.DontSave;
        }
    }

    private IEnumerator SweepCastTarget()
    {
        if (castTargetInstance == null) yield break;

        while (isCharging && castTargetInstance != null)
        {
            // use global phase so slider and target remain synchronized
            float phase = Mathf.PingPong((Time.time - chargeStartTime) * targetSweepSpeed, 1f); // 0..1..0
            currentTargetDistance = Mathf.Lerp(minCastDistance, maxCastDistance, phase);
            castTargetInstance.transform.position = castOrigin.position + castOrigin.forward * currentTargetDistance;
            yield return null;
        }
    }

    private void DestroyCastTarget()
    {
        if (castTargetInstance != null)
        {
            Destroy(castTargetInstance);
            castTargetInstance = null;
        }
    }

    private void StartCast(float overrideForce = -1f)
    {
        // require no active hook and that line is not currently cast
        if (hook.activeSelf || isFishing || canReel)
        {
            if (showDebugLogs)
                Debug.Log($"Cannot cast fishing hook: Already casting or hook active. Hook: {hook.activeSelf}, isCasting: {isFishing}, Can Reel: {canReel}");
            return;
        }

        prevRbUseGravity = PlayerComponents.rb.useGravity;
        prevRbKinematic = PlayerComponents.rb.isKinematic;
        prevRbConstraints = PlayerComponents.rb.constraints;

        if (showDebugLogs) Debug.Log("Casting fishing hook.");

        isFishing = true;
        castTime = Time.time;

        // mark the moment the hook started its cast for use in failsafe check
        fishingStartTime = Time.time;

        // Enable the hook object
        if (showDebugLogs) Debug.Log("Enabling hook.");
        hook.transform.SetPositionAndRotation(castOrigin.position, Quaternion.identity);
        hook.SetActive(true);
        if (!hook.TryGetComponent<Rigidbody>(out var rb))
        {
            Debug.LogError("Fishing Hook has no Rigidbody component.");
            return;
        }

        // ensure there is a HookController on the prefab
        if (showDebugLogs) Debug.Log("Initializing HookController.");
        if (!hook.TryGetComponent<HookController>(out currentHookController))
        {
            Debug.LogError("Fishing Hook has no HookController component.");
            return;
        }

        currentHookController.Initialize(OnHooked, OnHookStopped, gameObject);

        // Ensure physics are reset before applying impulse
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = true;

        // If a pending target position was captured, compute initial velocity to land on that position.
        if (hasPendingTarget)
        {
            Vector3 origin = castOrigin.position;
            Vector3 target = pendingTargetPosition;

            Vector3 toTarget = target - origin;
            // compute time-to-target based on horizontal distance and a speed factor (castForce used as speed baseline)
            Vector3 toTargetXZ = new(toTarget.x, 0f, toTarget.z);
            float horizontalDistance = toTargetXZ.magnitude;
            float speedBaseline = (overrideForce > 0f ? overrideForce : castForce);
            float time = Mathf.Clamp(horizontalDistance / Mathf.Max(0.1f, speedBaseline), 0.25f, 3f);

            // required initial velocity:
            Vector3 initialVelocity = toTarget / time - 0.5f * time * Physics.gravity;

            // apply velocity directly so the hook follows physics and lands near target
            rb.linearVelocity = initialVelocity;
            rb.angularVelocity = Vector3.zero;

            hasPendingTarget = false;
        }
        else
        {
            Debug.LogError("No pending target position found for cast.)");
        }

        // enable line
        line.enabled = true;

        // don't allow immediate reeling — enable after distance or delay
        canReel = false;
        input = false;
    }

    private void Update()
    {
        if (hook == null)
        {
            line.enabled = false;
            return;
        }

        float distance = Vector3.Distance(castOrigin.position, hook.transform.position);

        if (disableMovementWhileFishing && isFishing)
        {
            // Disable movement while fishing
            PlayerComponents.characterController.Move(Vector3.zero); // Ensure no residual movement
            PlayerComponents.rb.isKinematic = false;
            PlayerComponents.rb.useGravity = false;
            PlayerComponents.rb.angularVelocity = Vector3.zero;
            PlayerComponents.rb.linearVelocity = Vector3.zero;
            PlayerComponents.rb.constraints = RigidbodyConstraints.FreezeAll;

            PlayerComponents.SetCertainComponents(!(isCharging || isFishing), source: gameObject, PlayerComponents.playerController);
        }

        // Failsafe Check
        if (hook.activeSelf && fishingStartTime > 0f && Time.time > (fishingStartTime + 3f))
        {
            
            if (distance >= maxCastDistance * 1.5f) // if hook somehow got flung very far (e.g. into a void), clean it up
            {
                if (showDebugLogs) Debug.LogWarning($"Hook position {hook.transform.position} is out of bounds. Cleaning up hook.");
                CleanupHook();
            }
        }


        // update charge slider while charging (make it sweep in sync with target)
        if (isCharging && castChargeSlider != null)
        {
            float phase = Mathf.PingPong((Time.time - chargeStartTime) * targetSweepSpeed, 1f);
            castChargeSlider.value = Mathf.Clamp01(phase);
        }

        // update line renderer from origin to hook
        if (line.enabled)
        {
            line.SetPosition(0, castOrigin.position);
            line.SetPosition(1, hook.transform.position);
        }

        if (currentHookController == null) return;

        // enable reeling when either:
        //  - the hook has physically moved a sufficient distance from the origin, or
        //  - a short time delay passed after the cast
        if (!canReel && isFishing)
        {
            if (distance >= minCastDistance || Time.time >= castTime + reelEnableDelay)
            {
                canReel = true;
                if (showDebugLogs) Debug.Log("Reeling enabled (hook traveled sufficient distance or time elapsed).");
            }
        }
    }

    private void FixedUpdate()
    {
        if (!canReel || currentHookController == null) return;

        float distance = Vector3.Distance(castOrigin.position, hook.transform.position);
        if (!currentHookController.HasHit && distance < minCastDistance)
        {
            // Hook is too close to player after being allowed to reel; retract
            Retract();
        }

        if (input)
        {
            ReelStep();
        }
        else if (!currentHookController.HasHit && distance > maxCastDistance)
        {
            // If hook exceeded max distance and hasn't hit anything, start reeling automatically
            ReelStep();
        }
    }

    private void ReelStep()
    {
        if (hook == null) return;

        // if an object is hooked, pull that object toward hand; otherwise retract the hook.
        if (currentHookController != null && currentHookController.HookedObject != null)
        {
            var hooked = currentHookController.HookedObject;
            // Move hook and hooked object together
            Vector3 target = castOrigin.position;
            hooked.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(hooked.transform.position, target, reelSpeed * Time.deltaTime));
            hook.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(hook.transform.position, target, reelSpeed * Time.deltaTime));

            float dist = Vector3.Distance(castOrigin.position, hooked.transform.position);
            if (dist <= pickupDistance)
            {
                // pick up object
                PickupHookedObject(hooked);
            }
        }
        else
        {
            Retract();
        }
    }

    private void Retract()
    {
        // retract hook
        hook.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(hook.transform.position, castOrigin.position, reelSpeed * Time.deltaTime));
        float dist = Vector3.Distance(castOrigin.position, hook.transform.position);
        if (dist <= 0.5f)
        {
            // cleanup hook when it reaches the player
            CleanupHook();
        }
    }

    private void OnHooked(GameObject hookedObject, HookController hook)
    {
        // do something when something gets hooked
        if (hook.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    private void OnHookStopped(HookController hookController)
    {
        // if hook destroyed or released
        CleanupHook();
    }

    private void PickupHookedObject(MoveableObject hooked)
    {
        // Pick up the reeled-in object
        PlayerComponents.playerEquipItem.UnequipItem(); // Unequip current item (the rod)
        hooked.OnPlayerInteraction(gameObject);
        if (showDebugLogs) Debug.Log($"Picked up hooked object: {hooked.gameObject.name}");
        CleanupHook();
    }

    private void CleanupHook()
    {
        if (isInCleanupState) return; // prevent multiple cleanups

        isInCleanupState = true;
        if (showDebugLogs) Debug.Log("Cleaning up fishing hook.");
        if (hook != null)
            hook.SetActive(false);
        currentHookController = null;
        line.enabled = false;
        canReel = false;
        isFishing = false;
        fishingStartTime = -1f;
        if (disableMovementWhileFishing)
        {
            // Reenable movement
            PlayerComponents.rb.constraints = prevRbConstraints;
            PlayerComponents.rb.linearVelocity = Vector3.zero;
            PlayerComponents.rb.angularVelocity = Vector3.zero;
            PlayerComponents.rb.isKinematic = prevRbKinematic;
            PlayerComponents.rb.useGravity = prevRbUseGravity;

            PlayerComponents.SetCertainComponents(!(isCharging || isFishing), source: gameObject, PlayerComponents.playerController);
        }

        // ensure any target artifacts are removed
        DestroyCastTarget();
        hasPendingTarget = false;

        PlayerComponents.SetCertainComponents(true, source: gameObject, PlayerComponents.playerFloating, PlayerComponents.playerPossessing, PlayerComponents.playerMantling);

        StartCoroutine(WaitForCastCooldown());
    }

    IEnumerator WaitForCastCooldown()
    {
        canCast = false;
        yield return new WaitForSeconds(castCooldown);
        if (showDebugLogs) Debug.Log("Fishing cooldown has finished");
        canCast = true;
        isInCleanupState = false;
    }

    bool HasRod() => playerEquipItem != null && playerEquipItem.currentEquippedItem != null && playerEquipItem.currentEquippedItem == fishingRod;
}