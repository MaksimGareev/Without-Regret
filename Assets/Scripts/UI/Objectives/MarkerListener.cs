using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MarkerListener : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;
    [SerializeField] List<Transform> objects;
    SceneLoadManager sceneManager;

    void Awake()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(ObjectiveActivated);
        sceneManager = GameManager.Instance.sceneLoadManager;
    }

    private void OnEnable()
    {
        sceneManager.OnSceneLoaded.AddListener(OnSceneLoad);
    }

    private void OnDisable()
    {
        sceneManager.OnSceneLoaded.RemoveListener(OnSceneLoad);
    }
    

    private void ObjectiveActivated(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            GameManager.Instance.GetComponentInChildren<MultiObjectMarkers>().AssignMarkers(objects);
        }
    }

    private void OnSceneLoad()
    {
        ObjectiveActivated(ObjectiveManager.Instance.GetActiveObjectives().FirstOrDefault());

    }
}
