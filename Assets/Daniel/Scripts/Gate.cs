using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private GameObject RotateRightDoor;
    [SerializeField] private GameObject RotateLeftDoor;
    [SerializeField] private ObjectiveData linkedObjective;
    public bool needsObjective = true;
    private bool isObjectiveActive = false;

    private Animator rightDoorAnimator;
    private Animator leftDoorAnimator;

    void Awake()
    {
        rightDoorAnimator = RotateRightDoor.GetComponent<Animator>();
        leftDoorAnimator = RotateLeftDoor.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveComplete);
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null || linkedObjective == null || isObjectiveActive) return;

        //isObjectiveActive = ObjectiveManager.Instance.IsObjectiveCompleted(linkedObjective.objectiveID);
    }

    private void SetObjectiveComplete(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            isObjectiveActive = true;
        }
    }
    
    
    void OnTriggerEnter(Collider other)
    {
        if (!isObjectiveActive && needsObjective) return;

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Found Player");

            rightDoorAnimator.SetBool("NearPlayer", true);
            leftDoorAnimator.SetBool("NearPlayer", true);
        }
    }
}
