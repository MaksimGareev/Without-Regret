using UnityEngine;
using Unity.AI.Navigation;
using System;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
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
        if (!TryGetComponent<Collider>(out coll))
        {
            disableColliderWhileHeld = false;
        }
    }

    public bool CanInteract(GameObject player)
    {
        if (isGrabbable == false || DialogueManager.DialogueIsActive)
            return false;

        // Make sure player is facing toward the object by getting the Dot Product
        Vector3 toObject = (transform.position - player.transform.position).normalized;
        if (Vector3.Dot(player.transform.forward, toObject) < 0.5f)
            return false;

        // Make sure player is within grabbing distance of the collider
        float distance = GetDistanceToCollider(coll, player.transform.position);
        if (distance > maxGrabDistance)
            return false;

        return true;
    }

    private void Grab(Transform grabTransform)
    {
        if (!isGrabbable) return;

        // Get closest point on collider to grab position (world space)
        Vector3 closestPoint = coll.ClosestPoint(grabTransform.position);

        // Calculate offset from object pivot to that closest point
        Vector3 pivotToClosest = closestPoint - transform.position;

        // Move object so closest point aligns with grab transform
        transform.position = grabTransform.position - pivotToClosest;

        // Parent to grab point
        transform.parent = grabTransform;

        IsGrabbed = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = false;

        if (disableColliderWhileHeld)
            coll.enabled = false;

        // Apply offsets after parenting (Rotate in world space to maintain rotation at time of grabbing)
        transform.localPosition += heldPositionOffset;

        // remove Icon
        if (ButtonIcons.Instance != null)
            ButtonIcons.Instance.Clear();
    }

    public void Release()
    {
        IsGrabbed = false;
        grabPoint = null;
        transform.parent = null; // Unparent from grabpoint
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (disableColliderWhileHeld)
            coll.enabled = true;

        // Set rigidbody back to normal
        rb.isKinematic = false;
        rb.WakeUp();
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

    private float GetDistanceToCollider(Collider collider, Vector3 point)
    {
        // Find the closest point on the collider's surface to the player's position
        Vector3 closestPoint = collider.ClosestPoint(point);

        // Calculate the distance between the player's position and that closest point
        float distance = Vector3.Distance(point, closestPoint);

        return distance;
    }

    public float GetMoveSlowdown() => moveSlowdownDivisor;
    public float GetSprintSlowdown() => sprintSlowdownDivisor;
    public float GetSprintDepletion() => sprintDepletionFactor;
    public float GetSprintTimerDecay() => staminaReduction;
    public bool GetAllowSprint() => allowSprint;
}
