using UnityEngine;
using UnityEngine.AI;

public class Darry : MonoBehaviour
{
    // Movement
    public NavMeshAgent agent;
    public Transform target;

    // objectives
    [SerializeField] ObjectiveData linkedHouseObjective;
    [SerializeField] ObjectiveData linkedNeighborhoodObjective;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            this.gameObject.SetActive(false);
            ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            Debug.Log("Darry has reached the door.");
        }

        if (other.CompareTag("finish"))
        {
            ObjectiveManager.Instance.AddProgress(linkedNeighborhoodObjective.objectiveID, 1);
            Debug.Log("Darry has made it to the end.");
        }
    }
}
