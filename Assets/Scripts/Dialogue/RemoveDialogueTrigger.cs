using UnityEngine;

public class RemoveDialogueTrigger : MonoBehaviour
{
    public string linkedObjectiveID;
    private ObjectiveManager objectiveManager;
    public GameObject trigger1;
    public GameObject trigger2;
    public GameObject trigger3;
    public GameObject Enemy;
    public NewDialogueTrigger DialogueTrigger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        objectiveManager = ObjectiveManager.Instance;
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

    public void RemoveGameobjects()
    {
        if (DialogueTrigger.completed && DialogueTrigger != null)
        {
            trigger1.SetActive(false);
            trigger2.SetActive(false);
            trigger3.SetActive(false);
            Enemy.SetActive(false);
        }
    }
}
