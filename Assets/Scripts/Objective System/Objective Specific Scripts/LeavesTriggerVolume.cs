using System.Collections.Generic;
using UnityEngine;

public class LeavesTriggerVolume : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;
    private List<GameObject> leavesInVolume = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Leaves") && !leavesInVolume.Contains(other.gameObject))
        {
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            leavesInVolume.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Leaves") && leavesInVolume.Contains(other.gameObject))
        {
            leavesInVolume.Remove(other.gameObject);
            ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, -1);
        }
    }
}
