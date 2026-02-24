using UnityEngine;
using Unity.AI.Navigation;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour, IInteractable
{
    [SerializeField, Tooltip("When moving this object, the player's base speed is divided by this value")] 
    private float moveSlowdownDivisor = 3f;
    [SerializeField, Tooltip("When moving this object, the player's sprinting speed is divided by this value")] 
    private float sprintSlowdownDivisor = 3f;
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

    public event Action OnInteracted;
    private Collider coll;

    [SerializeField] private NavMeshSurface navMeshSurface;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        coll = GetComponent<Collider>();
    }

    public bool CanInteract(GameObject player)
    {
        if (isGrabbable == false)
            return false;

        if (DialogueManager.DialogueIsActive)
            return false;

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

        // Set Moveable Object position to the grabPoint
        grabPoint = grabTransform;
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

        // Set rigidbody back to normal
        rb.isKinematic = false;
        rb.WakeUp();
    }

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
    }

    public void OnPlayerInteraction(GameObject player)
    {
        if (!player.TryGetComponent<PlayerMovingObjects>(out mover)) return;
        
        PlayerInteracting interacting = player.GetComponent<PlayerInteracting>();

        // Prevent interaction if requiredItem is not in player inventory
        if (requiredItem != null && player.TryGetComponent<Inventory>(out var items))
        {
            if (!items.OtherItems.Contains(requiredItem))
            {
                Debug.Log($"Player tried to move {gameObject.name}, but is missing required item {requiredItem.ItemName}");
                return;
            }
        }

        // Only get grabbed if the player isnt holding something already
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
                Debug.LogWarning("Player tried to grab an object while having an item equipped.");
                return;
            }
            // Move to grabPoint
            Grab(mover.grabPoint);
            mover.OnMovingObject(this);
            interacting.SetHeldObject(this);
        }
        else
        {
            // Object is current held, so release it instead
            Release();
            mover.OnReleaseObject(this);
            interacting.ClearHeldObjects();
        }

        // Notify any listeners
        OnInteracted?.Invoke();
    }

    public float GetMoveSlowdown() => moveSlowdownDivisor;
    public float GetSprintSlowdown() => sprintSlowdownDivisor;
    public float GetSprintDepletion() => sprintDepletionFactor;
    public float GetSprintTimerDecay() => staminaReduction;
    public bool GetAllowSprint() => allowSprint;
}
