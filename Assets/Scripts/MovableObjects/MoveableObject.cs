using UnityEngine;
using System;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MoveableObject : MonoBehaviour, IInteractable
{
    [SerializeField, Tooltip("When moving this object, the player's base speed is divided by this value")] 
    private float moveSlowdownDivisor = 1.75f;
    [SerializeField, Tooltip("When moving this object, the player's sprinting speed is divided by this value")] 
    private float sprintSlowdownDivisor = 1.5f;
    [SerializeField, Tooltip("Modifies how quickly the player's sprint timer is reduced when this object is held while sprinting. Higher number = Faster reduction")] 
    private float sprintDepletionFactor = 1.05f;
    [SerializeField, Tooltip("Modifies how quickly the player's sprint timer is reduced when this object is held while moving (but not sprinting). Higher number = faster reduction")]
    private float staminaReduction = 0.35f;
    [SerializeField, Range(0, 1), Tooltip("Determines how strict the angle between the player forward vector and object must be to allow interaction. 1 = player can be facing parallel, 0 = player must be perfectly perpendicular")]
    private float dotProductThreshold = 0.4f;
    [SerializeField] private float maxGrabDistance = 2.5f;

    [Header("Options")]
    [SerializeField] private bool allowSprint = true;
    [SerializeField] ItemData requiredItem;
    [Min(0f), Tooltip("When the player is moving this object, the size of the collider used to check for collisions with the environment is multiplied by this factor.")]
    [SerializeField] private float collisionCheckSizeFactor = 1f;
    [SerializeField] private bool checkGrabPointCollisions = true;

    [Header("Transform Settings")]
    [SerializeField] private Vector3 heldPositionOffset = Vector3.zero;
    [SerializeField] private bool setHeldRotation = false;
    [SerializeField] private Vector3 heldRotation = Vector3.zero;
    [SerializeField] private bool applyHeldRotationOffset = false;
    [SerializeField] private Vector3 heldRotationOffset = Vector3.zero;

    private Rigidbody rb;
    private PlayerMovingObjects mover;
    private NavMeshObstacle navMeshObstacle;

    public bool IsGrabbed { get; private set; } = false;
    public bool isGrabbable = true;
    public float interactionPriority => 5f;
    public InteractType interactType => InteractType.Move;

    public event Action OnInteracted;
    private Collider coll;
    private bool trigger;

    public Collider ObjectCollider => coll;
    public float CollisionCheckSizeFactor => collisionCheckSizeFactor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();

        trigger = coll.isTrigger;
        if (TryGetComponent<NavMeshObstacle>(out var obstacle))
        {
            navMeshObstacle = obstacle;
        }
    }

    public bool CanInteract(GameObject player)
    {
        if (isGrabbable == false || DialogueManager.DialogueIsActive)
            return false;

        // Make sure player is facing toward the object by getting the Dot Product
        Vector3 toObject = (transform.position - player.transform.position).normalized;
        if (Vector3.Dot(player.transform.forward, toObject) < dotProductThreshold)
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
        rb.isKinematic = true;

        coll.isTrigger = true;
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = false;
        }

        // Apply offsets after parenting (Rotate in world space to maintain rotation at time of grabbing)
        transform.localPosition += heldPositionOffset;
        if (setHeldRotation)
        {
            transform.localRotation = Quaternion.Euler(heldRotation);
        }
        if (applyHeldRotationOffset)
        {
            transform.Rotate(heldRotationOffset, Space.World);
        }

        // remove Icon
        if (ButtonIcons.Instance != null)
            ButtonIcons.Instance.Clear();
    }

    public void Release()
    {
        IsGrabbed = false;
        transform.parent = null; // Unparent from grabpoint
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Set rigidbody back to normal
        rb.isKinematic = false;
        rb.WakeUp();

        coll.isTrigger = trigger;
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = true;
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

            // Attempt to ensure that when the object is grabbed, there isn't major clipping with another object
            // by checking the grab point for collisions.
            if (checkGrabPointCollisions && mover.grabPoint.TryGetComponent<GrabPointCollisionCheck>(out var checker) && checker.CollidingWithSomethingExcept(coll))
            {
                Debug.Log($"Player tried to grab an object, but their grab point is colliding with another object");
                return;
            }

            // Can't grab if an item is equipped
            if (PlayerComponents.playerEquipItem != null && PlayerComponents.playerEquipItem.currentEquippedItem != null)
            {
                Debug.LogWarning("Player tried to grab an object while having an item equipped.");
                return;
            }
            // Prevent collision with the player before grabbing
            Physics.IgnoreCollision(coll, player.GetComponent<Collider>(), true);
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
            // Reenable collision with the player after releasing
            Physics.IgnoreCollision(coll, player.GetComponent<Collider>(), false);
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
