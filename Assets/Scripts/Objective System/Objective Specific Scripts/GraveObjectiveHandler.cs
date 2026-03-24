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

        // Cache gravestones and wire up initial subscriptions/disable as appropriate
        gravestones = GameObject.FindGameObjectsWithTag("Gravestone");

        if (!isObjectiveActive)
        {
            // For initial inactive state we still want to subscribe to OnSuccess so we catch completion
            foreach (GameObject gravestone in gravestones)
            {
                if (gravestone.TryGetComponent<PushableObject>(out var pushable))
                {
                    // Ensure we don't double-subscribe
                    pushable.OnSuccess -= IncrementCount;
                    pushable.OnSuccess += IncrementCount;
                }

                StartCoroutine(WaitToDisableGravestone(gravestone));
            }
        }
        else
        {
            // If objective already active, ensure gravestones are enabled and subscribed
            foreach (GameObject gravestone in gravestones)
            {
                rb = gravestone.GetComponent<Rigidbody>();
                if (gravestone.TryGetComponent<PushableObject>(out var pushable))
                {
                    pushable.isInteractable = true;
                    pushable.OnSuccess -= IncrementCount;
                    pushable.OnSuccess += IncrementCount;
                }
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
            }
        }
    }

    private IEnumerator WaitToDisableGravestone(GameObject gravestone)
    {
        yield return new WaitForSeconds(1f);
        Debug.Log($"Disabling gravestone interaction for {gravestone.name} at start of scene since objective is not active.");
        PushableObject pushable = gravestone.GetComponent<PushableObject>();
        Rigidbody rb = gravestone.GetComponent<Rigidbody>();
        if (pushable != null)
        {
            pushable.isInteractable = false;
            if (rb != null)
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

        gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
        foreach (GameObject gravestone in gravestones)
        {
            PushableObject pushable = gravestone.GetComponent<PushableObject>();
            rb = gravestone.GetComponent<Rigidbody>();

            if (pushable != null && !pushable.IsComplete)
            {
                Debug.Log($"Enabling gravestone interaction for {gravestone.name} since objective is now active.");
                pushable.isInteractable = true;
                // ensure subscription once
                pushable.OnSuccess -= IncrementCount;
                pushable.OnSuccess += IncrementCount;
            }

            if (rb != null)
            {
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

        // refresh cached gravestones
        gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
        foreach (GameObject gravestone in gravestones)
        {
            rb = gravestone.GetComponent<Rigidbody>();

            if (gravestone.TryGetComponent<PushableObject>(out var pushable))
            {
                Debug.Log($"Disabling gravestone interaction for {gravestone.name} since objective is now inactive.");
                pushable.isInteractable = false;
                pushable.OnSuccess -= IncrementCount;
            }

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        isObjectiveActive = false;
    }

    private void IncrementCount()
    {
        if (Time.timeSinceLevelLoad > 1f) // Prevents adding progress if reloading save and gravestone is already placed in the correct position from previous session.
        {
            Debug.Log("Gravestone placed in correct position, adding progress to objective.");
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
    }
}
