using UnityEngine;

public class ObjectiveTesting : MonoBehaviour
{
    [SerializeField] private ObjectiveData testObjective1;
    [SerializeField] private ObjectiveData testObjective2;
    [SerializeField] private ObjectiveData testObjective3;
    private string currentObjectiveID;

    void Update()
    {
        // Press 1 to activate first objective
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ObjectiveManager.Instance.ActivateObjective(testObjective1);
            currentObjectiveID = testObjective1.objectiveID;
        }

        // Press 2 to activate second objective
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ObjectiveManager.Instance.ActivateObjective(testObjective2);
            currentObjectiveID = testObjective2.objectiveID;
        }

        // Press 3 to activate second objective
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ObjectiveManager.Instance.ActivateObjective(testObjective3);
            currentObjectiveID = testObjective3.objectiveID;
        }

        // Press UpArrow to add progress to the current objective
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ObjectiveManager.Instance.AddProgress(currentObjectiveID, 1);
        }
    }
}
