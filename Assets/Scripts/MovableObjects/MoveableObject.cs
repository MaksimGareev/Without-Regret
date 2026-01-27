using UnityEngine;
using System.Collections;
using Unity.AI.Navigation;
using System.Reflection;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float moveSlowdownMultiplier = 3f;
    private PlayerMovingObjects playerMovingObjects; 
    private Transform grabPoint;
    private Rigidbody rb;
    public bool IsGrabbed { get; private set; } = false;
    public bool isGrabbable = true;
    public float InteractionPriority => 1;
    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;
    private Collider coll;

    // [SerializeField] private NavMeshSurface navMeshSurface;
    // Ground Check Parameters
    private float groundCheckDistance = 0.1f; // extra ray distance below collider
    private float groundVelocityThreshold = 0.01f; // velocity threshold to consider 'stopped'


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
    }

    private void Grab(Transform grabPoint)
    {
        this.grabPoint = grabPoint;
        IsGrabbed = true;

        rb.isKinematic = true;

        DisablePopupIcon();
    }

    public void Release()
    {
        IsGrabbed = false;
        grabPoint = null;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        EnablePopupIcon();

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
        /*
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
        */
    }

    private void Update()
    {
        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }
    }

    public void OnPlayerInteraction(GameObject player)
    {
        playerMovingObjects = player.GetComponent<PlayerMovingObjects>();

        if (!IsGrabbed && isGrabbable)
        {
            Grab(playerMovingObjects.grabPoint);
            playerMovingObjects.OnMovingObject(moveSlowdownMultiplier);
        }
        else
        {
            Release();
            playerMovingObjects.OnReleaseObject();
        }
    }

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
    }
}
