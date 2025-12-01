using UnityEngine;
using System.Collections;
using Unity.AI.Navigation;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private float moveSlowdownMultiplier = 3f;
    private PlayerMovingObjects playerMovingObjects; 
    private Transform grabPoint;
    private Rigidbody rb;
    public bool isGrabbed { get; private set; } = false;
    public bool isGrabbable = true;
    public float interactionPriority => 1;
    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

   // [SerializeField] private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Grab(Transform grabPoint)
    {
        this.grabPoint = grabPoint;
        isGrabbed = true;

        rb.isKinematic = true;

        DisablePopupIcon();
    }

    public void Release()
    {
        isGrabbed = false;
        grabPoint = null;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
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
        if (!isGrabbed && rb.linearVelocity.magnitude < 0.01f && rb.angularVelocity.magnitude < 0.01f)
        {
            //rb.Sleep();
        }
        
        if (isGrabbed && grabPoint != null)
        {
            rb.MovePosition(grabPoint.position);
            rb.MoveRotation(grabPoint.rotation);
        }
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

        if (!isGrabbed && isGrabbable)
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
}
