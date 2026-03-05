using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MoveableObjectPlacement : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Objective data for the objective this slot is linked to. This is used to check if the objective is ACTIVE and to add progress when a moveable object is placed.")]
    [SerializeField] private ObjectiveData linkedObjective;

    [Tooltip("Reference to the moveable object that the player can pick up and place in this slot. Should be the instance in the scene, not a prefab.")]
    [SerializeField] private GameObject moveableObjectInstance;

    [Tooltip("Ghost prefab to show the player where to put the moveable object. Will be made transparent automatically.")]
    [SerializeField] private GameObject ghostPrefab;

    [Tooltip("Transform child object that indicates the position and rotation where the moveable object will snap to when placed in this slot.")]
    [SerializeField] private Transform lockingPosition;

    [Header("Settings")]
    [Tooltip("Whether this placement slot requires the linked objective to be active in order for the player to place the moveable object here. If set to false, the player will be able to place the moveable object here even if the linked objective is not active.")]
    [SerializeField] private bool needsObjective;

    [Tooltip("Whether to add progress to the linked objective when the player places a moveable object in this slot.")]
    [SerializeField] private bool addProgress;
    
    [Tooltip("Whether to rebuild the NavMesh after placing the moveable object in this slot. Leave this unchecked if the moveable object is not an obstacle that NPCs need to navigate around or on, to save on performance.")]
    [SerializeField] private bool rebuildNavMesh = false;

    private NavMeshSurface[] navMeshSurfaces;
    private GameObject player;
    private PlayerMovingObjects playerMovingObjects;
    private MoveableObject moveableObjectScript;
    private Rigidbody rb;
    private bool isObjectiveActive = false;
    private bool didOnce = false;
    private GameObject ghostInstance;
    private bool ghostEnabled = false;
    private float ghostAlpha = 0.7f;

    [SerializeField] private TraversablePoint[] traversablePoints;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveInactive);
    }

    private void Start()
    {
        // Finding and assigning references, with warnings if something isn't set up correctly in the inspector.
        if (lockingPosition == null)
        {
            Debug.LogWarning("No Transform found for locking position.");
        }

        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerMovingObjects = player.GetComponent<PlayerMovingObjects>();
        }
        else
        {
            Debug.LogWarning("Player not found.");
        }

        if (moveableObjectInstance != null)
        {
            moveableObjectScript = moveableObjectInstance.GetComponent<MoveableObject>();
        }
        else
        {
            Debug.LogWarning("No moveable object instance linked in inspector!");
        }

        if (rebuildNavMesh)
        {
            navMeshSurfaces = FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);

            foreach (var surface in navMeshSurfaces)
            {
                Debug.Log("NavMeshSurface found: " + surface.gameObject.name);
            }

            if (navMeshSurfaces.Length == 0)
            {
                Debug.LogWarning("Rebuild NavMesh is enabled but no NavMeshSurfaces were found in the scene.");
            }
        }
        
        if (!isObjectiveActive && needsObjective && linkedObjective != null && ObjectiveManager.Instance != null)
        {
            CheckObjectiveStatus();
            StartCoroutine(WaitToDisableMovableObject(moveableObjectInstance));
        }

        InitializeGhost();
    }

    private void Update()
    {
        if ((!isObjectiveActive && needsObjective) || ghostInstance == null || moveableObjectScript == null) return;

        // Show ghost if player is carrying an object and ghost isn't already enabled
        if (playerMovingObjects != null && !ghostEnabled && moveableObjectScript.IsGrabbed)
        {
            EnableGhost();
        }
        // Hide ghost if player isn't carrying an object and ghost is currently enabled.
        else if (playerMovingObjects != null && ghostEnabled && !moveableObjectScript.IsGrabbed)
        {
            DisableGhost();
        }
    }

    private void CheckObjectiveStatus()
    {
        // Check if the linked objective is already active at the start (i.e. on reloading save), update the isObjectiveActive bool
        if (linkedObjective != null && ObjectiveManager.Instance != null)
        {
            isObjectiveActive = ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID);
        }
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        // Enable the moveable object's grabbability when the linked objective is activated
        if (objective.data != linkedObjective)
        {
            return;
        }

        isObjectiveActive = true;

        MoveableObject moveable = moveableObjectInstance.GetComponent<MoveableObject>();
        rb = moveableObjectInstance.GetComponent<Rigidbody>();

        if (moveable != null && rb != null)
        {
            moveable.isGrabbable = true;
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    private void SetObjectiveInactive(ObjectiveInstance objective)
    {
        // Update isObjectiveActive bool. Called by the listener when the ObjectiveManager handles an objective's completion.
        if (objective.data != linkedObjective)
        {
            return;
        }

        isObjectiveActive = false;
    }

    private IEnumerator WaitToDisableMovableObject(GameObject movableobject)
    {
        if (isObjectiveActive) yield break;

        // Wait for level to load and object's placement to settle, then disable grabbability and freeze the object in place until the linked objective is activated.
        yield return new WaitForSeconds(3f);
        MoveableObject moveable = movableobject.GetComponent<MoveableObject>();
        Rigidbody rigidbody = movableobject.GetComponent<Rigidbody>();
        if (moveable != null && rigidbody != null)
        {
            moveable.isGrabbable = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isObjectiveActive && needsObjective)
        {
            return;
        }

        if (other.gameObject == moveableObjectInstance)
        {
            MoveableObject moveableObject = other.GetComponent<MoveableObject>();
            rb = moveableObject?.GetComponent<Rigidbody>();

            if (moveableObject != null && !didOnce)
            {
                if (moveableObject.IsGrabbed)
                {
                    // Auto-release object from the player's hand
                    moveableObject.OnPlayerInteraction(player);
                }
                // Lock object to the locking position
                moveableObject.isGrabbable = false;
                moveableObject.transform.position = lockingPosition.position;
                moveableObject.transform.rotation = lockingPosition.rotation;

                for(int i = 0; i<traversablePoints.Length; ++i)
                {
                    traversablePoints[i].isTraversable = true; //sets traversable points on protected NPC path to true
                }

                // Add progress to the objective if appropriate
                if (Time.timeSinceLevelLoad > 1f && addProgress && linkedObjective != null && ObjectiveManager.Instance != null)
                {
                    ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                }

                // Freeze the object in place and prevent further interactions with it, disable ghost, rebuild navmesh and save
                rb.constraints = RigidbodyConstraints.FreezeAll;
                didOnce = true;
                DisableGhost();
                StartCoroutine(RebuildNavMesh());
                if (SaveManager.Instance != null) 
                {
                    SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
                }
            }
        }
    }

    private void InitializeGhost()
    {
        // Create ghost instance and set it up (make transparent, disable colliders, remove rigidbody if it has one)
        if (ghostPrefab != null && lockingPosition != null)
        {
            ghostInstance = Instantiate(ghostPrefab, lockingPosition.position, lockingPosition.rotation);
            MakeGhostTransparent(ghostInstance, ghostAlpha);
            ghostInstance.SetActive(false);

            Collider[] cols = ghostInstance.GetComponentsInChildren<Collider>();
            foreach (Collider col in cols)
            {
                col.enabled = false;
            }

            Rigidbody rb = ghostInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }
        }
    }

    private void MakeGhostTransparent(GameObject ghost, float alpha)
    {
        if (ghostInstance != null)
        {
            Renderer[] renderers = ghostInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = alpha; // Set transparency level
                        mat.color = color;

                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                }
            }
        }
    }

    private void EnableGhost()
    {
        if (ghostInstance != null)
        {
            MakeGhostTransparent(ghostInstance, ghostAlpha);
            ghostInstance.transform.position = lockingPosition.position;
            ghostInstance.transform.rotation = lockingPosition.rotation;
            ghostInstance.SetActive(true);
            ghostEnabled = true;
        }
    }

    private void DisableGhost()
    {
        if (ghostInstance != null)
        {
            ghostInstance.SetActive(false);
            ghostEnabled = false;
        }
    }

    private IEnumerator RebuildNavMesh()
    {
        if (!rebuildNavMesh || navMeshSurfaces == null || navMeshSurfaces.Length == 0) 
        {
            //Debug.LogWarning("Rebuild NavMesh is disabled or no NavMeshSurfaces found, skipping NavMesh rebuild.");
            yield break;
        }

        // wait for object to stop moving
        while (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
        {
            yield return null;
            //Debug.Log("Waiting for object to settle before rebuilding NavMesh...");
        }

        // Rebuild all NavMeshSurfaces in the scene
        foreach (NavMeshSurface surface in navMeshSurfaces)
        {
            surface.BuildNavMesh();
            //Debug.Log("Rebuilding NavMesh on surface: " + surface.gameObject.name);
        }

        // Update all NavMeshLinks in the scene to ensure they connect properly to the updated NavMesh
        foreach (var link in FindObjectsByType<NavMeshLink>(FindObjectsSortMode.None))
        {
            link.UpdateLink();
            //Debug.Log("Updating NavMeshLink: " + link.gameObject.name);
        }
    }

    private void OnDisable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.RemoveListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(SetObjectiveInactive);
        StopAllCoroutines();
    }
}
