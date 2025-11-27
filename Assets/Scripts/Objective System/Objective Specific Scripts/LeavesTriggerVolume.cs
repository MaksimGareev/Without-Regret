using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavesTriggerVolume : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;
    private List<GameObject> leavesInVolume = new List<GameObject>();

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveInactive);
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            foreach (GameObject leaf in GameObject.FindGameObjectsWithTag("Leaves"))
            {
                MoveableObject moveable = leaf.GetComponent<MoveableObject>();
                Rigidbody rb = leaf.GetComponent<Rigidbody>();
                if (moveable != null)
                {
                    moveable.isGrabbable = true;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                }
            }
        }
    }

    private void SetObjectiveInactive(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            foreach (GameObject leaf in GameObject.FindGameObjectsWithTag("Leaves"))
            {
                StartCoroutine(WaitToDisableLeafGrabbable(leaf));
            }
        }
    }

    private IEnumerator WaitToDisableLeafGrabbable(GameObject leaf)
    {
        yield return new WaitForSeconds(3f);
        MoveableObject moveable = leaf.GetComponent<MoveableObject>();
        Rigidbody rb = leaf.GetComponent<Rigidbody>();
        if (moveable != null)
        {
            moveable.isGrabbable = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

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
