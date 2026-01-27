using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;


public class DarrysHouseObjective : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;
    private Transform lockingPosition;
    private GameObject player;
    private bool isObjectiveActive = false;
    private Rigidbody rb;
    private bool didOnce = false;
    public bool needsObjective = false;

    [SerializeField] private NavMeshSurface navMeshSurface;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveInactive);
    }

    private void Start()
    {
        lockingPosition = GetComponentInChildren<Transform>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (lockingPosition == null)
        {
            Debug.LogWarning("No Transform found for locking position.");
        }

        if (player == null)
        {
            Debug.LogWarning("Player not found.");
        }

        if (linkedObjective == null)
        {
            Debug.LogWarning("No objective linked in inspector!");
        }

        if (!isObjectiveActive && needsObjective)
        {
            GameObject[] Crates = GameObject.FindGameObjectsWithTag("Crates");
            foreach (GameObject Crate in Crates)
            {
                StartCoroutine(WaitToDisableGravestone(Crate));
            }
        }
        if (!needsObjective)
        {
            SetObjectiveActive();
        }
    }

    private void Update()
    {
        GameObject[] Crates = GameObject.FindGameObjectsWithTag("Crates");
        foreach (GameObject Crate in Crates)
        {
            MoveableObject moveable = Crate.GetComponent<MoveableObject>();
        }
        
    }

    private IEnumerator WaitToDisableGravestone(GameObject crate)
    {
        yield return new WaitForSeconds(3f);
        MoveableObject moveable = crate.GetComponent<MoveableObject>();
        Rigidbody rb = crate.GetComponent<Rigidbody>();
        if (moveable != null)
        {
            moveable.isGrabbable = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isObjectiveActive && needsObjective)
        {
            return;
        }

        if (other.CompareTag("Crates"))
        {
            MoveableObject gravestone = other.GetComponent<MoveableObject>();
            rb = gravestone.gameObject.GetComponent<Rigidbody>();

            if (gravestone != null && !didOnce)
            {
                gravestone.OnPlayerInteraction(player);
                gravestone.isGrabbable = false;
                other.gameObject.transform.position = lockingPosition.position;
                other.gameObject.transform.rotation = lockingPosition.rotation;
                ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                rb.constraints = RigidbodyConstraints.FreezeAll;
                didOnce = true;

            }

            // Rebuild NavMesh after the object is moved
            if (navMeshSurface != null)
            {
                //navMeshSurface.BuildNavMesh();
                StartCoroutine(RebuildNavMeshWhenStill());
            }
        }
    }

    private IEnumerator RebuildNavMeshWhenStill()
    {
        // wait for object to stop moving
        while (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
        {
            yield return null;
        }

        navMeshSurface.BuildNavMesh();

        foreach (var link in FindObjectsOfType<NavMeshLink>())
        {
            link.UpdateLink();
        }
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data != linkedObjective && needsObjective)
        {
            return;
        }

        isObjectiveActive = true;

        GameObject[] Crates = GameObject.FindGameObjectsWithTag("Crates");
        foreach (GameObject Crate in Crates)
        {
            MoveableObject moveable = Crate.GetComponent<MoveableObject>();
            rb = Crate.GetComponent<Rigidbody>();

            if (moveable != null)
            {
                moveable.isGrabbable = true;
                rb.constraints = RigidbodyConstraints.None;
            }
        }
    }

    private void SetObjectiveActive()
    {
        isObjectiveActive = true;

        GameObject[] Crates = GameObject.FindGameObjectsWithTag("Crates");
        foreach (GameObject Crate in Crates)
        {
            MoveableObject moveable = Crate.GetComponent<MoveableObject>();
            rb = Crate.GetComponent<Rigidbody>();

            if (moveable != null)
            {
                moveable.isGrabbable = true;
                rb.constraints = RigidbodyConstraints.None;
            }
        }
    }

    private void SetObjectiveInactive(ObjectiveInstance objective)
    {
        if (objective.data != linkedObjective)
        {
            return;
        }

        isObjectiveActive = false;
    }
}
