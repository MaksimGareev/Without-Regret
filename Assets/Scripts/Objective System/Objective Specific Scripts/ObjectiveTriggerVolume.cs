using UnityEngine;

public class ObjectiveTriggerVolume : MonoBehaviour
{
    [SerializeField] private ObjectiveData linkedObjective;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && linkedObjective != null && ObjectiveManager.Instance != null)
        {
            var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
            
            foreach (var obj in activeObjectives)
            {
                if (obj.data == linkedObjective)
                {
                    obj.AddProgress(1);
                }
            }
        }
    }
}
