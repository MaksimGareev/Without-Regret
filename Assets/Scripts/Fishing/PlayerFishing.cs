using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerFishing : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference castAction;
    [SerializeField] private InputActionReference reelAction;

    [Header("References")]
    [SerializeField] private Transform castOrigin;          // origin of cast
    [SerializeField] private GameObject hook;            // hook object. Should be in the scene
    [SerializeField] private PlayerEquipItem playerEquipItem;  // used to attach reeled object

    [Header("Throw / Reel")]
    [SerializeField] private float castForce = 20f;
    [SerializeField] private float maxCastDistance = 15f;
    [SerializeField] private float minCastDistance = 2f;
    [SerializeField] private float reelSpeed = 8f;
    [SerializeField] private float retractSpeed = 12f; // when nothing hooked
    [SerializeField] private float pickupDistance = 1.0f; // distance to auto-hold

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showLineRenderer = true;

    private LineRenderer line;
    private HookController currentHookController;
    private bool canReel = false;
    private bool reelInput = false;
    private bool isCasting = false;

    private void OnEnable()
    {
        castAction.action.Enable();
        reelAction.action.Enable();
    }
    private void OnDisable()
    {
        castAction.action.Disable();
        reelAction.action.Disable();
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
        if (castAction != null)
        {
            castAction.action.started += ctx => OnCastStarted(ctx);
        }
        if (reelAction != null)
        {
            reelAction.action.performed += ctx => OnReel(ctx);
            reelAction.action.canceled += ctx => OnReel(ctx);
        }
    }

    private void OnCastStarted(InputAction.CallbackContext ctx)
    {
        // require no active hook and that line is not currently cast
        if (!ctx.action.triggered || hook != null || isCasting || canReel) return;

        // require rod equipped
        if (playerEquipItem == null || !(playerEquipItem.currentEquippedItem.ItemType == ItemType.FishingRod)) return;

        StartCast();
    }

    private void OnReel(InputAction.CallbackContext ctx)
    {
        if (hook == null || !canReel) return;
        
        reelInput = ctx.action.triggered;

        if (showDebugLogs) Debug.Log("Reel Input triggered");
    }

    private void StartCast()
    {
        if (showDebugLogs) Debug.Log("Casting fishing hook.");

        isCasting = true;

        // enable hook
        hook.transform.position = castOrigin.position;
        hook.SetActive(true);
        if (hook.TryGetComponent<Rigidbody>(out var rb)) rb = hook.AddComponent<Rigidbody>();
        else if (showDebugLogs) Debug.LogError("Fishing Hook has no Rigidbody component.");

        // ensure there is a HookController on the prefab
        currentHookController = hook.GetComponent<HookController>();
        if (currentHookController == null) currentHookController = hook.AddComponent<HookController>();
        currentHookController.Initialize(OnHooked, OnHookStopped);

        // launch forward from camera / hand direction
        Vector3 dir = castOrigin.forward;
        rb.AddForce(dir.normalized * castForce, ForceMode.Impulse);

        // enable line
        if (showLineRenderer) line.enabled = true;

        canReel = true;
    }

    private void StopCast()
    {
        if (showDebugLogs) Debug.Log("Stopping fishing cast.");

        Retract();

        isCasting = false;
        canReel = false;

        // disable hook
        hook.SetActive(false);
        line.enabled = false;
    }

    private void Update()
    {
        if (hook == null)
        {
            line.enabled = false;
            return;
        }

        // update line renderer from origin to hook
        if (showLineRenderer && line.enabled)
        {
            line.SetPosition(0, castOrigin.position);
            line.SetPosition(1, hook.transform.position);
        }

        if (!currentHookController.HasHit && Vector3.Distance(castOrigin.position, hook.transform.position) < minCastDistance)
        {
            // Hook is too close to player, retract hook
            StopCast();

        }
        
        if (reelInput)
        {
            ReelStep();
        }
        else if (!currentHookController.HasHit && Vector3.Distance(castOrigin.position, hook.transform.position) > maxCastDistance)
        {
            // If hook exceeded max distance and hasn't hit anything, start retracting automatically
            ReelStep();
        }
    }

    private void ReelStep()
    {
        if (hook == null) return;

        // if an object is hooked, pull that object towards hand; otherwise retract the hook.
        if (currentHookController.HookedObject != null)
        {
            var hooked = currentHookController.HookedObject;
            // Move hooked object toward player hand (use move towards to avoid tunnelling)
            Vector3 target = castOrigin.position;
            hooked.transform.position = Vector3.MoveTowards(hooked.transform.position, target, reelSpeed * Time.deltaTime);

            // Also pull the hook to the object for consistent line rendering
            hook.transform.position = hooked.transform.position;

            float dist = Vector3.Distance(castOrigin.position, hooked.transform.position);
            if (dist <= pickupDistance)
            {
                // pick up object
                PickupHookedObject(hooked);
            }
        }
        else
        {
            StopCast();
        }
    }

    private void Retract()
    {
        // retract hook to hand
        hook.transform.position = Vector3.MoveTowards(hook.transform.position, castOrigin.position, retractSpeed * Time.deltaTime);
        float dist = Vector3.Distance(castOrigin.position, hook.transform.position);
        if (dist <= 0.25f)
        {
            StopCast();
        }
    }

    private void OnHooked(GameObject hookedObject, HookController hook)
    {
        // do something when something gets hooked
        var rb = hook.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
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
        StopCast();
    }

    private void CleanupHook()
    {
        hook.SetActive(false);
        currentHookController = null;
        line.enabled = false;
        canReel = false;
        isCasting = false;
    }

    // Simple helper component for the hook object; detects collisions with moveable objects
    // and notifies the owner. Placed inside same file for simplicity.
    public class HookController : MonoBehaviour
    {
        public MoveableObject HookedObject { get; private set; }
        public bool HasHit => HookedObject != null;

        private Action<GameObject, HookController> onHooked;
        private Action<HookController> onStopped;

        public void Initialize(Action<GameObject, HookController> onHookedCb, Action<HookController> onStoppedCb)
        {
            onHooked = onHookedCb;
            onStopped = onStoppedCb;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (HookedObject != null) return;

            if (collision.gameObject.TryGetComponent<MoveableObject>(out var moveable) && moveable.isGrabbable)
            {
                HookedObject = moveable;
                onHooked?.Invoke(collision.gameObject, this);
            }
        }

        private void OnDisable()
        {
            onStopped?.Invoke(this);
        }
    }
}