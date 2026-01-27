using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerFishing : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference fishingAction;

    [Header("References")]
    [SerializeField] private Transform castOrigin;          // origin of cast
    [SerializeField] private GameObject hook;            // hook object. Should be in the scene
    [SerializeField] private PlayerEquipItem playerEquipItem;  // used to attach reeled object

    [Header("Cast / Reel")]
    [SerializeField] private float castForce = 20f;
    [SerializeField] private float maxCastDistance = 15f;
    [SerializeField] private float minCastDistance = 2f;
    [SerializeField] private float reelSpeed = 8f;
    [SerializeField] private float pickupDistance = 3.0f; // distance to auto-hold
    [SerializeField] private float castCooldown = 0.5f; // time after the hook is cleaned up until it can be cast again

    [Header("Reel Enabling")]
    [SerializeField] private float reelEnableDistance = 2.5f; // hook must travel this far before reeling is allowed
    [SerializeField] private float reelEnableDelay = 0.15f;   // short delay before enabling reel

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showLineRenderer = true;

    private LineRenderer line;
    private HookController currentHookController;
    private bool canReel = false;
    private bool canCast = true;
    private bool input = false;
    private bool isCasting = false;

    private float _castTime;

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

        if (hook == null)
        {
            Debug.LogError("PlayerFishing: Hook prefab reference is missing.");
        }
        else
        {
            hook.SetActive(false);
        }

        // input action subscriptions
        if (fishingAction != null)
        {
            fishingAction.action.performed += ctx => OnInput(ctx);
            fishingAction.action.canceled += ctx => OnInput(ctx);
        }
    }

    private void OnInput(InputAction.CallbackContext ctx)
    {
        if (hook == null)
        {
            Debug.LogError("PlayerFishing: Hook prefab reference is missing.");
            return;
        }
        
        input = ctx.action.triggered;

        if (showDebugLogs) Debug.Log($"Fishing Input triggered {input}");
    }

    private void StartCast()
    {
        // require no active hook and that line is not currently cast
        if (hook.activeSelf || isCasting || canReel)
        {
            if (showDebugLogs)
                Debug.Log($"Cannot cast fishing hook: Already casting or hook active. Hook: {hook.activeSelf}, isCasting: {isCasting}, Can Reel: {canReel}");
            return;
        }

        if (showDebugLogs) Debug.Log("Casting fishing hook.");

        isCasting = true;
        _castTime = Time.time;

        // Enable the hook object
        if (showDebugLogs) Debug.Log("Enabling hook.");
        hook.transform.position = castOrigin.position;
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

        currentHookController.Initialize(OnHooked, OnHookStopped);

        // launch forward from camera / hand direction
        Vector3 dir = castOrigin.forward;
        rb.AddForce(dir.normalized * castForce, ForceMode.Impulse);

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
            Debug.LogError("PlayerFishing: Hook prefab reference is missing."); 
            return;
        }

        // Cast the line
        if (canCast && !canReel && !isCasting && HasRod() && input)
        {
            StartCast();
            isCasting = true;
        }
        else if (!canReel && input)
        {
            Debug.Log($"Cannot cast: Can Cast: {canCast} Is Casting: {isCasting}, Has Rod: {HasRod()}");
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
            if (distance >= reelEnableDistance || Time.time >= _castTime + reelEnableDelay)
            {
                canReel = true;
                if (showDebugLogs) Debug.Log("Reeling enabled (hook traveled sufficient distance or delay elapsed).");
            }
        }

        // Only consider "too close -> retract" after reeling has been enabled.
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
        // retract hook to hand
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