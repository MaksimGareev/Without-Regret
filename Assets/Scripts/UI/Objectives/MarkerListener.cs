using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MarkerListener : MonoBehaviour
{
    [SerializeField] ObjectiveData linkedObjective;
    [SerializeField] List<Transform> objects;
    [SerializeField] MultiObjectMarkers multiObjectMarkers;
    SceneLoadManager sceneManager;

    void Awake()
    {
        if (ObjectiveManager.Instance)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(ObjectiveActivated);
        }

        if (GameManager.Instance)
        {
            sceneManager = GameManager.Instance.sceneLoadManager;
            if (!multiObjectMarkers)
            {
                multiObjectMarkers = GameManager.Instance.GetComponentInChildren<MultiObjectMarkers>(true);
            }
        }
    }

    private void OnEnable()
    {
        if (!sceneManager && GameManager.Instance)
        {
            sceneManager = GameManager.Instance.sceneLoadManager;
        }

        sceneManager?.OnSceneLoaded.AddListener(OnSceneLoad);
    }

    private void OnDisable()
    {
        sceneManager?.OnSceneLoaded.RemoveListener(OnSceneLoad);
    }
    

    private void ObjectiveActivated(ObjectiveInstance objective)
    {
        if (objective == null || objective.data == null || linkedObjective == null)
        {
            return;
        }

        if (objects == null || objects.Count <= 0)
        {
            return;
        }

        if (objective.data != linkedObjective)
        {
            return;
        }

        if (!multiObjectMarkers && GameManager.Instance)
        {
            multiObjectMarkers = GameManager.Instance.GetComponentInChildren<MultiObjectMarkers>(true);
        }

        if (!multiObjectMarkers)
        {
            Debug.LogWarning($"{nameof(MarkerListener)} could not find {nameof(MultiObjectMarkers)} in children of GameManager.");
            return;
        }

        List<Transform> validObjects = objects.Where(t => t != null).ToList();
        if (validObjects.Count == 0)
        {
            Debug.LogWarning($"{nameof(MarkerListener)} has no valid marker targets for objective {linkedObjective.name}.");
            return;
        }

        multiObjectMarkers.AssignMarkers(validObjects);
        Debug.Log($"Objective Marker Activated ({validObjects.Count} targets)");
    }

    private void OnSceneLoad()
    {
        if (!ObjectiveManager.Instance)
        {
            return;
        }

        ObjectiveInstance activeObjective = ObjectiveManager.Instance.GetActiveObjectives()?.FirstOrDefault();
        if (activeObjective == null)
        {
            return;
        }

        ObjectiveActivated(activeObjective);
    }
}
