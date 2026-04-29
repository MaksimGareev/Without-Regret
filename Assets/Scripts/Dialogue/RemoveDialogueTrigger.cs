using UnityEngine;

public class RemoveDialogueTrigger : MonoBehaviour
{
    public ObjectiveData linkedObjective;
    public ObjectiveData mediationObjective;
    public GameObject trigger1;
    public GameObject trigger2;
    //public GameObject trigger3;
    public GameObject Enemy;
    public NewDialogueTrigger DialogueTrigger;

    private void Start()
    {
        if (!CheckIfObjectiveActive() || !DialogueTrigger) return;
        if (Enemy) Enemy.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered");
        }

        if (this.CompareTag("Intro"))
        {
            if (other.CompareTag("Player"))
            {
                trigger1.SetActive(false);
            }
        }
    }

    public void Update()
    {
        RemoveGameobjects();
    }

    private bool CheckIfObjectiveActive()
    {
        if (!linkedObjective || !ObjectiveManager.Instance) return false;
        
        return ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID) || ObjectiveManager.Instance.IsObjectiveCompleted(mediationObjective.objectiveID);
    }

    public void RemoveGameobjects()
    {
        if ((DialogueTrigger && DialogueTrigger.completed) || !CheckIfObjectiveActive())
        {
            if (trigger1&& trigger1.activeSelf) trigger1.SetActive(false);
            if (trigger2&& trigger2.activeSelf) trigger2.SetActive(false);
            //if (trigger3&& trigger3.activeSelf) trigger3.SetActive(false);
            if (Enemy && Enemy.activeSelf) Enemy.SetActive(false);
        }
        else if (DialogueTrigger && !DialogueTrigger.completed && CheckIfObjectiveActive())
        {
            if (trigger1 && !trigger1.activeSelf) trigger1.SetActive(true);
            if (trigger2 && !trigger2.activeSelf) trigger2.SetActive(true);
            //if (trigger3 && !trigger3.activeSelf) trigger3.SetActive(true);
        }
    }
}
