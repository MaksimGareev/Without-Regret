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

    private void OnEnable()
    {
        if (needsObjective && linkedObjective != null)
        {
            if (ObjectiveManager.Instance != null)
            {
                if (ObjectiveManager.Instance.IsObjectiveCompleted(linkedObjective.objectiveID))
                {
                    locked = false;
                }
                else
                {
                    ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveComplete);
                }
            }
        }
    }

    private void SetObjectiveComplete(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            locked = false;
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
            return;
        }
        else
        {
            Debug.Log($"Loading Gate: locked={state.locked}, opened={state.opened}, ID: {GetUniqueID()}");
        }

        bool objLockCheck = ObjectiveManager.Instance.IsObjectiveCompleted(linkedObjective.objectiveID);

        locked = objLockCheck ? true : state.locked;
        opened = state.opened;

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
