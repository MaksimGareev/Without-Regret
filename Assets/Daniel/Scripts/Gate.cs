using UnityEngine;
using System;

public class Gate : MonoBehaviour, ISaveable
{
    [SerializeField] private GameObject RotateRightDoor;
    [SerializeField] private GameObject RotateLeftDoor;
    [SerializeField] private ObjectiveData linkedObjective;
    public bool needsObjective = true;
    private string uniqueID;
    private bool locked = true;
    private bool opened = false;

    private Animator rightDoorAnimator;
    private Animator leftDoorAnimator;

    private void Awake()
    {
        rightDoorAnimator = RotateRightDoor.GetComponent<Animator>();
        leftDoorAnimator = RotateLeftDoor.GetComponent<Animator>();

        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }

    public string GetUniqueID() => uniqueID;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveComplete);
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
        }
    }

    public void SaveTo(SaveData data)
    {
        GateSaveData state = new GateSaveData();

        state.id = uniqueID;
        state.locked = locked;
        state.opened = opened;
    }

    public void LoadFrom(SaveData data)
    {
        var state = data.gateListSaveData.gates.Find(g => g.id == uniqueID);

        if (state == null) 
        {
            Debug.LogWarning("No save data found for Gate with ID: " + uniqueID);
            return;
        }

        locked = state.locked;
        opened = state.opened;

        if (opened)
        {
            rightDoorAnimator.SetBool("NearPlayer", true);
            leftDoorAnimator.SetBool("NearPlayer", true);
        }
    }
}
