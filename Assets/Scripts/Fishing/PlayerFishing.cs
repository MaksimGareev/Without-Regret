using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class PlayerFishing : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference fishingAction;

    [Header("References")]
    [SerializeField] private Transform castOrigin;
    [SerializeField] private PlayerEquipItem playerEquipItem;  // used to attach reeled object
    [SerializeField] private Slider castChargeSlider;

    private GameObject hook;            // hook object. Should be the only child of the Fishing Rod prefab

    [Header("Cast / Reel")]
    [SerializeField] private float castForce = 10f;
    [SerializeField] private float maxCastDistance = 15f;
    [SerializeField] private float minCastDistance = 2f;
    [SerializeField] private float upwardArcFactor = 1.75f; // upward component multiplier for the cast's launch arc
    [SerializeField] private float reelSpeed = 8f;
    [SerializeField] private float pickupDistance = 3.0f; // distance to auto-hold
    [SerializeField] private float castCooldown = 0.5f; // time after the hook is cleaned up until it can be cast again
    [SerializeField] private bool disableMovementWhileFishing = true;

    [Header("Charge")]
    [SerializeField] private float maxChargeTime = 1.5f; // time to reach full charge (maxCastDistance)

    [Header("Reel Enabling")]
    [SerializeField] private float reelEnableDistance = 2.5f; // hook must travel this far before reeling is allowed
    [SerializeField] private float reelEnableDelay = 0.75f;   // short delay before enabling reel

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showLineRenderer = true;

    private LineRenderer line;
    private HookController currentHookController;
    private bool canReel = false;
    private bool canCast = true;
    private bool input = false;         // whether the cast/reel button is currently held (for reeling)
    private bool isCasting = false;     // whether the hook is currently cast/out
    private bool isCharging = false;

    private bool prevRbUseGravity;
    private bool prevRbKinematic;

    private float castTime;
    private float chargeStartTime;

    private void OnEnable()
    {
        fishingAction.action.Enable();
    }
    private void OnDisable()
    {
        fishingAction.action.Disable();
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

        // input action subscriptions
        if (fishingAction != null)
        {
            fishingAction.action.started += OnInputStarted;
            fishingAction.action.canceled += OnInputCanceled;
        }

        if (!PlayerComponents.initialized) PlayerComponents.InitializeComponents(gameObject);

    }

    private void OnInputStarted(InputAction.CallbackContext ctx)
    {
        if (!HasRod()) return;

        if (hook == null)
        {
            if (HasRod())
            {
                hook = playerEquipItem.GetEquippedItemInstance().transform.GetChild(0).gameObject;
            }
            else
            {
                Debug.LogError("PlayerFishing: Couldn't find a hook");
                return;
            }
        }

        // If player is not currently casting and is allowed to cast, start charging
        if (canCast && !canReel && !isCasting && HasRod() && !hook.activeSelf)
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
            if (showDebugLogs) Debug.Log($"Cannot start cast or reel: CanCast={canCast} IsCasting={isCasting} HasRod={HasRod()}");
        }
    }

    private void OnInputCanceled(InputAction.CallbackContext ctx)
    {
        if (!HasRod()) return;
        if (hook == null)
        {
            Debug.LogError("PlayerFishing: Hook prefab reference is missing.");
            return;
        }

        // If we were charging, release to cast
        if (isCharging)
        {
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
    }

    private void ReleaseCharge()
    {
        if (!isCharging) return;

        float chargeTime = Time.time - chargeStartTime;
        float normalized = Mathf.Clamp01(maxChargeTime <= 0f ? 1f : chargeTime / maxChargeTime);

        // Scale force by charge. Ensure a small minimum so very short taps still cast.
        float forceFactor = Mathf.Lerp(0.2f, 1f, normalized);
        float finalForce = castForce * forceFactor;

        if (showDebugLogs) Debug.Log($"Released charge. Charge: {normalized:F2}, ForceFactor: {forceFactor:F2}, FinalForce: {finalForce:F2}");

        isCharging = false;
        if (castChargeSlider != null)
        {
            castChargeSlider.value = 0f;
            castChargeSlider.gameObject.SetActive(false);
        }

        // perform the cast with computed force
        StartCast(finalForce);
    }

    private void StartCast(float overrideForce = -1f)
    {
        // require no active hook and that line is not currently cast
        if (hook.activeSelf || isCasting || canReel)
        {
            if (showDebugLogs)
                Debug.Log($"Cannot cast fishing hook: Already casting or hook active. Hook: {hook.activeSelf}, isCasting: {isCasting}, Can Reel: {canReel}");
            return;
        }

        prevRbUseGravity = PlayerComponents.rb.useGravity;
        prevRbKinematic = PlayerComponents.rb.isKinematic;

        if (showDebugLogs) Debug.Log("Casting fishing hook.");

        isCasting = true;
        castTime = Time.time;

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

        // launch forward with computed force at an upward arc
        Vector3 dir = castOrigin.forward;
        float forceToUse = overrideForce > 0f ? overrideForce : castForce;
        float upward = forceToUse * upwardArcFactor;
        Vector3 totalImpulse = dir.normalized * forceToUse + castOrigin.up * upward;
        rb.AddForce(totalImpulse, ForceMode.Impulse);

        // enable line
        if (showLineRenderer) line.enabled = true;

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

        if (disableMovementWhileFishing)
        {
            // Disable movement while fishing
            PlayerComponents.characterController.Move(Vector3.zero); // Ensure no residual movement
            PlayerComponents.rb.isKinematic = false;
            PlayerComponents.rb.useGravity = false;
            PlayerComponents.rb.angularVelocity = Vector3.zero;
            PlayerComponents.rb.linearVelocity = Vector3.zero;
            PlayerComponents.rb.constraints = RigidbodyConstraints.FreezeAll;

            PlayerComponents.SetCertainComponents(!(isCharging || isCasting), source: gameObject, PlayerComponents.playerController);
        }
        

        // update charge slider while charging
        if (isCharging && castChargeSlider != null)
        {
            float normalized = Mathf.Clamp01(maxChargeTime <= 0f ? 1f : (Time.time - chargeStartTime) / maxChargeTime);
            castChargeSlider.value = normalized;
        }

        // update line renderer from origin to hook
        if (showLineRenderer && line.enabled)
        {
            line.SetPosition(0, castOrigin.position);
            line.SetPosition(1, hook.transform.position);
        }

        if (currentHookController == null) return;

        // enable reeling when either:
        //  - the hook has physically moved a sufficient distance from the origin, or
        //  - a short time delay passed after the cast (helps with slow-launch physics)
        if (!canReel && isCasting)
        {
            float distance = Vector3.Distance(castOrigin.position, hook.transform.position);
            if (distance >= reelEnableDistance || Time.time >= castTime + reelEnableDelay)
            {
                canReel = true;
                if (showDebugLogs) Debug.Log("Reeling enabled (hook traveled sufficient distance or delay elapsed).");
            }
        }

        // Only consider if hook is too close after reeling has been enabled.
        if (canReel && !currentHookController.HasHit)
        {
            float distance = Vector3.Distance(castOrigin.position, hook.transform.position);
            if (distance < minCastDistance)
            {
                // Hook is too close to player after being allowed to reel -> retract
                Retract();
            }
        }

        if (canReel && input)
        {
            ReelStep();
        }
        else if (canReel && !currentHookController.HasHit && Vector3.Distance(castOrigin.position, hook.transform.position) > maxCastDistance)
        {
            // If hook exceeded max distance and hasn't hit anything, start reeling automatically
            ReelStep();
        }
    }

    private void ReelStep()
    {
        if (hook == null) return;

        // if an object is hooked, pull that object toward hand; otherwise retract the hook.
        if (currentHookController.HookedObject != null)
        {
            var hooked = currentHookController.HookedObject;
            // Move hook and hooked object together
            Vector3 target = castOrigin.position;
            hooked.transform.position = Vector3.MoveTowards(hooked.transform.position, target, reelSpeed * Time.deltaTime);
            hook.transform.position = Vector3.MoveTowards(hook.transform.position, castOrigin.position, reelSpeed * Time.deltaTime);

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
        hook.transform.position = Vector3.MoveTowards(hook.transform.position, castOrigin.position, reelSpeed * Time.deltaTime);
        float dist = Vector3.Distance(castOrigin.position, hook.transform.position);
        if (dist <= 0.25f)
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
        hooked.OnPlayerInteraction(gameObject);
        CleanupHook();
    }

    private void CleanupHook()
    {
        if (showDebugLogs) Debug.Log("Cleaning up fishing hook.");
        hook.SetActive(false);
        currentHookController = null;
        line.enabled = false;
        canReel = false;
        isCasting = false;
        if (disableMovementWhileFishing)
        {
            // Reenable movement
            PlayerComponents.rb.constraints = RigidbodyConstraints.None;
            PlayerComponents.rb.linearVelocity = Vector3.zero;
            PlayerComponents.rb.angularVelocity = Vector3.zero;
            PlayerComponents.rb.isKinematic = prevRbKinematic;
            PlayerComponents.rb.useGravity = prevRbUseGravity;

            PlayerComponents.SetCertainComponents(!(isCharging || isCasting), source: gameObject, PlayerComponents.playerController);
        }
        StartCoroutine(WaitForCastCooldown());
    }

    IEnumerator WaitForCastCooldown()
    {
        canCast = false;
        yield return new WaitForSeconds(castCooldown);
        if (showDebugLogs) Debug.Log("Fishing cooldown has finished");
        canCast = true;
    }

    bool HasRod() => !(playerEquipItem == null || playerEquipItem.currentEquippedItem == null || !playerEquipItem.currentEquippedItem.ItemName.Equals("Fishing Rod"));
}