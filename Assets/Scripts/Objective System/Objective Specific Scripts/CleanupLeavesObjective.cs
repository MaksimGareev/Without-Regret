using System.Collections.Generic;
using UnityEngine;

public class CleanupLeavesObjective : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;

    [Header("Leaf Objective Item Assignments")]
    [SerializeField] private ItemData LeavesReward;
    [SerializeField] private ItemData TrashbagRefForRemoval;
    private Inventory playerInv;
    private int NumLeavesCollected;
    private float interactSpamDelay = 2.0f;
    private float interactSpamTimer = 0.0f;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveInactive);
    }

    private void Start()
    {
        if(playerInv == null)
        {
            playerInv = (Inventory)FindFirstObjectByType(typeof(Inventory));
        }

        // If the objective is already active (e.g. player is reloading a save), make sure the leaves are interactable
        if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
        {
            SetObjectiveActive(new ObjectiveInstance(linkedObjective));
        }
    }

    private void Update()
    {
        if (interactSpamTimer < interactSpamDelay + 2.0f)
        {
            interactSpamTimer += Time.deltaTime;
        }
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            // Reload progress if player already has some from reloading a save
            for (int i = 0; i < objective.currentProgress; i++)
            {
                AddLeaves();
            }
            
            foreach (GameObject leaf in GameObject.FindGameObjectsWithTag("Leaves"))
            {
                if (leaf.TryGetComponent<RemoveableObject>(out var interactable) && leaf.gameObject.activeSelf)
                {
                    interactable.SetInteractable(true);
                    interactable.OnInteracted += IncrementCount; // Subscribe to the interaction event
                }
            }
        }
    }

    private void SetObjectiveInactive(ObjectiveInstance objective)
    {
        if (objective.data != linkedObjective) return;
        
        foreach (GameObject leaf in GameObject.FindGameObjectsWithTag("Leaves"))
        {
            if (leaf.TryGetComponent<RemoveableObject>(out var interactable))
            {
                interactable.SetInteractable(false);
                interactable.OnInteracted -= IncrementCount; // Unsubscribe from the interaction event
            }
        }
            
        if (playerInv.HasItemInInventory(TrashbagRefForRemoval))
        {
            playerInv.RemoveItem(TrashbagRefForRemoval);
        }
        
        if (!playerInv.HasItemInInventory(LeavesReward))
        {
            playerInv.AddItem(LeavesReward);
        }
    }

    void IncrementCount()
    {
        if (interactSpamTimer < interactSpamDelay) return;
        
        ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
        //Debug.Log("Collected 1 Leaf");
        interactSpamTimer = 0.0f;
    }

    public void AddLeaves()
    {
        if (interactSpamTimer < interactSpamDelay) return;
        
        NumLeavesCollected++;

        if (NumLeavesCollected >= 5)
        {
            playerInv.AddItem(LeavesReward);
            playerInv.RemoveItem(TrashbagRefForRemoval);
        }
    }
}
