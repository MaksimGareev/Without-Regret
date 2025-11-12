using System.Linq;
using UnityEngine;

public class GraveObjectiveSlot : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;
    private Transform lockingPosition;
    private GameObject player;
    private bool isObjectiveActive = false;
    private Rigidbody rb;
    private bool didOnce = false;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isObjectiveActive)
        {
            return;
        }

        if (other.CompareTag("Gravestone"))
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
        }
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data != linkedObjective)
        {
            return;
        }

        isObjectiveActive = true;
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
