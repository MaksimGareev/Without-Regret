using UnityEngine;
using Unity.AI.Navigation;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour, IInteractable
{
    // [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float moveSlowdownDivisor = 3f;
    [SerializeField] private float sprintSlowdownDivisor = 3f;
    [SerializeField, Tooltip("Modifies how quickly the player's sprint timer is reduced when this object is held while sprinting. Higher number = Faster reduction")] 
    private float sprintDepletionFactor = 1.05f;
    [SerializeField, Tooltip("Modifies how quickly the player's sprint timer is reduced when this object is held while moving (but not sprinting). Higher number = faster reduction")]
    private float staminaReduction = 0.5f;
    [SerializeField] private bool allowSprint = true;
    [SerializeField] ItemData requiredItem;
    [SerializeField] private float maxGrabDistance = 2.5f;

    [Header("Transform Settings")]
    [SerializeField, Tooltip("Certain transform offsets can cause overlapping colliders and lead to weird player movement, so this option is here as a failsafe")] 
    private bool disableColliderWhileHeld = true;
    [SerializeField] private Vector3 heldPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 heldRotationOffset = Vector3.zero;

    private Transform grabPoint;
    private Rigidbody rb;
    private PlayerMovingObjects mover;

    public bool IsGrabbed { get; private set; } = false;
    public bool isGrabbable = true;
    public float interactionPriority => 5f;
    public InteractType interactType => InteractType.Move;

    [Header("Popup Settings")]
    [SerializeField] private float iconDistance = 3f;
    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;

    public event Action OnInteracted;
    private GameObject popupInstance;
    private Collider coll;
    private Transform player;

    [SerializeField] private NavMeshSurface navMeshSurface;
    // Ground Check Parameters
    private float groundCheckDistance = 0.1f; // extra ray distance below collider
    private float groundVelocityThreshold = 0.01f; // velocity threshold to consider 'stopped'


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        coll = GetComponent<Collider>();
    }

    public void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("MoveableObject: Player not found! Make sure the Player has the 'Player' tag.", this);
        }
    }

    public bool CanInteract(GameObject player)
    {
        if (isGrabbable == false)
            return false;

        if (DialogueManager.DialogueIsActive)
            return false;

        /*if (rb == null || rb.isKinematic)
            return false;*/

        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist > maxGrabDistance)
            return false;

        Vector3 toObject = (transform.position - player.transform.position).normalized;
        if (Vector3.Dot(player.transform.forward, toObject) < 0.4f)
            return false;

        return true;
    }

    private void Grab(Transform grabTransform)
    {
        if (!isGrabbable) return;

        grabPoint = grabTransform;
        //this.grabPoint = grabPoint;
        IsGrabbed = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        if (disableColliderWhileHeld)
            coll.enabled = false;

        // Apply offsets so object snaps into the intended held pose
        ApplyHeldOffsets();

        // remove Icon
        if (ButtonIcons.Instance != null)
            ButtonIcons.Instance.Clear();
    }

    private void ApplyHeldOffsets()
    {
        if (grabPoint == null) return;

        // Position: interpret heldPositionOffset in grabPoint local space
        // Rotation: apply rotation offset relative to grabPoint rotation
        transform.SetPositionAndRotation(grabPoint.TransformPoint(heldPositionOffset), grabPoint.rotation * Quaternion.Euler(heldRotationOffset));
    }

    public void Release()
    {
        IsGrabbed = false;
        grabPoint = null;

        if (disableColliderWhileHeld)
            coll.enabled = true;


        rb.isKinematic = false;
        rb.WakeUp();
        //rb.linearVelocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;

        /*
        // reshow Icon if close to item
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= iconDistance)
        {
            ButtonIcons.Instance.Highlight(interactType);
        }
        else
        {
            ButtonIcons.Instance.Clear();
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        EnablePopupIcon();*/
    }

    public void EnablePopupIcon()
    {
        if (popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
            shouldShowIcon = true;
        }
    }

    public void DisablePopupIcon()
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
            popupInstance = null;
            shouldShowIcon = false;
        }
    }

    /*
    private void FixedUpdate()
    {
        if (IsGrabbed && grabPoint != null)
        {
            rb.MovePosition(grabPoint.position);
            rb.MoveRotation(grabPoint.rotation);
        }
        else if (IsOnGround())
        {
            // zero velocities and put the body to sleep if it's nearly stopped
            if (rb.linearVelocity.sqrMagnitude > groundVelocityThreshold * groundVelocityThreshold ||
                rb.angularVelocity.sqrMagnitude > 0.0001f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // stop simulation until an external wake event occurs
            rb.Sleep();
        }

        // Debugging
        
        if (!IsGrabbed && IsOnGround() && rb.linearVelocity != Vector3.zero)
        {
            Debug.LogWarning($"MovableObject {gameObject.name} is moving at Linear Velocity: {rb.linearVelocity} while not grabbed.");
        }
        else if (!IsGrabbed && IsOnGround())
        {
            // Not grabbed and is on ground; shouldn't be moving
            Debug.DrawLine(coll.bounds.center, coll.bounds.center + Vector3.up * (coll.bounds.extents.y + groundCheckDistance), Color.green);
        }
        else if (!IsGrabbed)
        {
            // Not grabbed and not on ground; issue if it should be on ground
            Debug.DrawLine(coll.bounds.center, coll.bounds.center + Vector3.up * (coll.bounds.extents.y + groundCheckDistance), Color.red);
        }
        
    }*/

    private void Update()
    {
        if (IsGrabbed && grabPoint != null)
        {
            // Apply the configured offsets so the held object position/rotation can be tuned.
            // Position offset is interpreted in the grabPoint's local space (TransformPoint).
            // Rotation offset is applied relative to grabPoint rotation.
            rb.MovePosition(grabPoint.position + heldPositionOffset);
            rb.MoveRotation(grabPoint.rotation * Quaternion.Euler(heldRotationOffset));
        }
        /*
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > iconDistance) return;

        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            ButtonIcons.Instance.Highlight(interactType);
        }
        else if (!shouldShowIcon && popupInstance != null )
        {
            ButtonIcons.Instance.Clear();
        }*/
    }

    public void OnPlayerInteraction(GameObject player)
    {
        mover = player.GetComponent<PlayerMovingObjects>();
        if (mover == null) return;
        
        PlayerInteracting interacting = player.GetComponent<PlayerInteracting>();

        if (requiredItem != null && player.TryGetComponent<Inventory>(out var items))
        {
            if (!items.OtherItems.Contains(requiredItem))
            {
                // Required item is not in inventory
                Debug.Log($"Player tried to move {gameObject.name}, but is missing required item {requiredItem.ItemName}");
                return;
            }
        }


        if (!IsGrabbed && !mover.IsOccupied())
        {
            if (mover.grabPoint == null)
            {
                Debug.LogError("Player grab point is null!");
                return;
            }
            // Can't grab if an item is equipped
            if (PlayerComponents.playerEquipItem.currentEquippedItem != null)
            {
                //Debug.LogWarning("Player tried to grab an object while having an item equipped.");
                return;
            }
            Grab(mover.grabPoint);
            mover.OnMovingObject(this);
            interacting.SetHeldObject(this);
        }
        else
        {
            Release();
            mover.OnReleaseObject(this);
            interacting.ClearHeldObjects();
        }

        // Notify any listeners
        OnInteracted?.Invoke();
    }

    /*
    private bool IsOnGround()
    {
        if (coll == null) return false;

        // Cast from collider center downwards to check for ground within expected range.
        Vector3 origin = coll.bounds.center;
        float castDistance = coll.bounds.extents.y + groundCheckDistance;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, castDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // also ensure the hit point is below the collider (safety)
            if (hit.point.y <= coll.bounds.min.y + groundCheckDistance + 0.0001f)
                return true;
        }

        return false;
    }*/

    public float GetMoveSlowdown() => moveSlowdownDivisor;
    public float GetSprintSlowdown() => sprintSlowdownDivisor;
    public float GetSprintDepletion() => sprintDepletionFactor;
    public float GetSprintTimerDecay() => staminaReduction;
    public bool GetAllowSprint() => allowSprint;
}
