using UnityEngine;
using System.Collections;
//using Unity.AI.Navigation;
using System.Reflection;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float moveSlowdownMultiplier = 3f;
    private PlayerMovingObjects playerMovingObjects; 
    private Transform grabPoint;
    private Rigidbody rb;
    private PlayerMovingObjects mover;
    public bool IsGrabbed { get; private set; } = false;
    public bool isGrabbable = true;
    public float interactionPriority => 5f;
    public InteractType interactType => InteractType.Move;

    [SerializeField] private float iconDistance = 3f;

    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    public event Action OnInteracted;
    private GameObject popupInstance;
    private Collider coll;

     private Transform player;

   // [SerializeField] private NavMeshSurface navMeshSurface;
    // Ground Check Parameters
    private float groundCheckDistance = 0.1f; // extra ray distance below collider
    private float groundVelocityThreshold = 0.01f; // velocity threshold to consider 'stopped'

    private bool isGrabbed;
    [SerializeField] private float maxGrabDistance = 2.5f;


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
        if (DialogueManager.DialogueIsActive)
            return false;

        if (isGrabbed)
            return false;

        if (rb == null || rb.isKinematic)
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

        grabPoint = grabTransform;
        //this.grabPoint = grabPoint;
        IsGrabbed = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        // remove Icon
        ButtonIcons.Instance.Clear();
    }

    public void Release()
    {
        IsGrabbed = false;
        grabPoint = null;

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

       /* // Rebuild NavMesh after the object is moved
        if (navMeshSurface != null)
        {
            //navMeshSurface.BuildNavMesh();
            StartCoroutine(RebuildNavMeshWhenStill());
        }*/
    }

   /* private IEnumerator RebuildNavMeshWhenStill()
    {
        // wait for object to stop moving
        while (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
        {
            yield return null;
        }

       // navMeshSurface.BuildNavMesh();
    }*/

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

    private void FixedUpdate()
    {
        if (IsGrabbed && grabPoint != null)
        {
            transform.position = grabPoint.position;
            transform.rotation = grabPoint.rotation;
        }

        /*
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > iconDistance) return;

        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            ButtonIcons.Instance.Highlight(interactType);
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            ButtonIcons.Instance.Clear();
        }*/
    }

    public void OnPlayerInteraction(GameObject player)
    {
        mover = player.GetComponent<PlayerMovingObjects>();
        if (mover == null) return;

        if (!IsGrabbed)
        {
            Grab(mover.grabPoint);
            mover.OnMovingObject(moveSlowdownMultiplier);
        }
        else
        {
            Release();
            mover.OnReleaseObject();
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
}
