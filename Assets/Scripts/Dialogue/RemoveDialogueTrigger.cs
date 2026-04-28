using UnityEngine;

public class RemoveDialogueTrigger : MonoBehaviour
{
    public ObjectiveData linkedObjective;
    public GameObject trigger1;
    public GameObject trigger2;
    public GameObject Enemy;
    public NewDialogueTrigger DialogueTrigger;

    private void Start()
    {
        trigger1.SetActive(false);
        trigger2.SetActive(false);
        Enemy.SetActive(false);
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
        
        return ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID);
    }

    public void RemoveGameobjects()
    {
        if ((!DialogueTrigger || !DialogueTrigger.completed) && CheckIfObjectiveActive()) return;
        
        trigger1.SetActive(false);
        trigger2.SetActive(false);
        Enemy.SetActive(false);
    }
}
