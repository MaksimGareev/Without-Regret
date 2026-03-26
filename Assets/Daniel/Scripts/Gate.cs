using UnityEngine;
using System;

public class Gate : SaveableWithID
{
    [Header("References")]
    [Tooltip("The door on the right side of the gate. This should be a child object of the gate with an Animator component that controls the door's opening and closing animations.")]
    [SerializeField] private GameObject RotateRightDoor;
    [Tooltip("The door on the left side of the gate. This should be a child object of the gate with an Animator component that controls the door's opening and closing animations.")]
    [SerializeField] private GameObject RotateLeftDoor;

    [Header("Objective Settings")]
    [Tooltip("Objective that must be COMPLETED to unlock the gate. If the player has not COMPLETED the linked objective, the gate will remain locked and will not open when the player walks towards it.")]
    [SerializeField] private ObjectiveData linkedObjective;
    [Tooltip("If false, the gate will be permanently unlocked and will not require an objective to be completed. If true, the gate will be locked until the linked objective is completed.")]
    public bool needsObjective = true;
    private bool locked = true;
    private bool opened = false;

    private Animator rightDoorAnimator;
    private Animator leftDoorAnimator;

    private void Awake()
    {
        rightDoorAnimator = RotateRightDoor.GetComponent<Animator>();
        leftDoorAnimator = RotateLeftDoor.GetComponent<Animator>();
    }

    private bool CheckIfObjectiveCompleted()
    {
        if (!needsObjective || !linkedObjective || !ObjectiveManager.Instance) return false;
        
        foreach (var objective in ObjectiveManager.Instance.GetCompletedObjectives())
        {
            if (objective.data == linkedObjective)
            {
                locked = false;
                Debug.Log($"Gate {GetUniqueID()}:Linked objective already completed. Unlocking gate.");
                return true;
            }
        }
            
        Debug.Log($"Gate {GetUniqueID()}:Linked objective not completed. Gate remains locked. Adding Listener for objective completion.");
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveComplete);
        return false;
    }

    private void SetObjectiveComplete(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            Debug.Log($"Gate {GetUniqueID()}:Linked objective completed from listener. Unlocking gate.");
            locked = false;
        }
        else
        {
            Debug.Log($"Gate {GetUniqueID()}: Objective completed, but it does not match the linked objective for this gate.");
        }
    }
    
    
    void OnTriggerEnter(Collider other)
    {
        if ((locked && needsObjective) || opened) return;

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Found Player");

            rightDoorAnimator.SetBool("NearPlayer", true);
            leftDoorAnimator.SetBool("NearPlayer", true);
            opened = true;
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }
    }

    public override void SaveTo(SaveData data)
    {
        GateSaveData state = new GateSaveData();

        state.id = GetUniqueID();
        state.locked = locked;
        state.opened = opened;

        if (data.gateListSaveData.gates.Exists(g => g.id == GetUniqueID()))
        {
            data.gateListSaveData.gates.RemoveAll(g => g.id == GetUniqueID());
        }

        data.gateListSaveData.gates.Add(state);

        Debug.Log($"Saving Gate: locked={state.locked}, opened={state.opened}, ID: {GetUniqueID()}");
    }

    public override void LoadFrom(SaveData data)
    {
        var state = data.gateListSaveData.gates.Find(g => g.id == GetUniqueID());

        if (state == null) 
        {
            Debug.LogWarning("Loading Failed: No save data found for Gate with ID: " + GetUniqueID());
        }
        else
        {
            Debug.Log($"Loading Gate: locked={state.locked}, opened={state.opened}, ID: {GetUniqueID()}");
        }

        if (CheckIfObjectiveCompleted())
        {
            locked = false;
        }
        else
        {
            locked = state == null || state.locked;
        }
        
        Debug.Log($"Locked state for gate with ID {GetUniqueID()} after checking objective: {locked}");
        
        opened = state?.opened ?? false;

        if (opened)
        {
            rightDoorAnimator.SetBool("NearPlayer", true);
            leftDoorAnimator.SetBool("NearPlayer", true);
        }
    }

    private void OnDisable()
    {
        if (needsObjective && linkedObjective != null && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(SetObjectiveComplete);
        }
    }
}
