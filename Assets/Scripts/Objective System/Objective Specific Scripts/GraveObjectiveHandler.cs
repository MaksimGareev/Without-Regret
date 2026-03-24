using System.Collections;
using UnityEngine;

public class GraveObjectiveHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Objective data for the objective this slot is linked to. This is used to check if the objective is ACTIVE and to add progress when a gravestone is put in place.")]
    [SerializeField] private ObjectiveData linkedObjective;
    private GameObject player;
    private bool isObjectiveActive = false;
    private Rigidbody rb;
    private bool didOnce = false;
    private GameObject[] gravestones;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveInactive);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Player not found.");
        }

        if (linkedObjective == null)
        {
            Debug.LogWarning("No objective linked in inspector!");
        }
        
        // Check if the linked objective is already active at the start (i.e. on reloading save), reenable gravestones.
        if (linkedObjective != null && ObjectiveManager.Instance != null)
        {
            if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
            {
                SetObjectiveActive(new ObjectiveInstance(linkedObjective));
            }
        }

        if (!isObjectiveActive)
        {
            GameObject[] gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
            foreach (GameObject gravestone in gravestones)
            {
                gravestone.GetComponent<PushableObject>().OnSuccess += IncrementCount;
                StartCoroutine(WaitToDisableGravestone(gravestone));
            }
        }

        gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
    }

    private IEnumerator WaitToDisableGravestone(GameObject gravestone)
    {
        yield return new WaitForSeconds(3f);
        PushableObject pushable = gravestone.GetComponent<PushableObject>();
        Rigidbody rb = gravestone.GetComponent<Rigidbody>();
        if (pushable != null)
        {
            pushable.isInteractable = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data != linkedObjective)
        {
            return;
        }

        isObjectiveActive = true;

        GameObject[] gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
        foreach (GameObject gravestone in gravestones)
        {
            PushableObject pushable = gravestone.GetComponent<PushableObject>();
            rb = gravestone.GetComponent<Rigidbody>();

            if (pushable != null)
            {
                pushable.isInteractable = true;
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
        foreach (GameObject gravestone in gravestones)
        {
            gravestone.GetComponent<PushableObject>().OnSuccess -= IncrementCount;
        }

        isObjectiveActive = false;
    }

    private void IncrementCount()
    {
        if (Time.timeSinceLevelLoad > 1f) // Prevents adding progress if reloading save and gravestone is already placed in the correct position from previous session.
        {
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
        }
        didOnce = true;

        SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
    }
}
