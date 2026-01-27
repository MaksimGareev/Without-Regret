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
    private PlayerMovingObjects mover;
    public bool isGrabbed { get; private set; } = false;
    public bool isGrabbable = true;
    public float interactionPriority => 1f;
    public InteractType interactType => InteractType.Move;

    [SerializeField] private float iconDistance = 3f;

    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

    private Transform player;

   // [SerializeField] private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

    private void Grab(Transform grabTransform)
    {
        grabPoint = grabTransform;
        //this.grabPoint = grabPoint;
        isGrabbed = true;

        rb.isKinematic = true;

        // remove Icon
        ButtonIcons.Instance.Clear();
    }

    public void Release()
    {
        isGrabbed = false;
        grabPoint = null;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

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
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > iconDistance) return;

       /* if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
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

        if (!isGrabbed && isGrabbable)
        {
            Grab(mover.grabPoint);
            mover.OnMovingObject(moveSlowdownMultiplier);
        }
        else
        {
            Release();
            mover.OnReleaseObject();
        }
    }
}
