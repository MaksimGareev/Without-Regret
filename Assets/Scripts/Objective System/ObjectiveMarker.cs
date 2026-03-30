using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class ObjectiveMarker : MonoBehaviour
{
    [Tooltip("In-world Objective Indicator")]
    public GameObject WorldIndicator;

    [Tooltip("UI indicator for offscreen objectives")]
    public OffscreenObjectiveIndicator ScreenSpaceIndicator;

    [Tooltip("Scenemanager to listen for event from")]
    public SceneLoadManager sceneManger;

    private int objectiveTransformIndex;

    private void OnEnable()
    {
        sceneManger.OnSceneLoaded.AddListener(OnSceneLoad);
    }

    private void OnDisable()
    {
        sceneManger.OnSceneLoaded.RemoveListener(OnSceneLoad);
    }

    private void Awake()
    {
        if (ScreenSpaceIndicator != null && WorldIndicator != null)
        {
            ScreenSpaceIndicator.target = WorldIndicator.transform;
        }
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(ObjectiveCompleted);
        ObjectiveManager.Instance.OnObjectiveProgressUpdated.AddListener(ObjectiveProgressed);
    }

    private void ObjectiveCompleted(ObjectiveInstance objective)
    {
        objectiveTransformIndex = 0;
        Refresh(objective, SceneManager.GetActiveScene());
    }

    private void Refresh(ObjectiveInstance objective, Scene scene)
    {
        if (!objective.data) return;
        
        if (objective.data.markerTransforms != null)
        {
            if (int.Equals(scene.buildIndex, objective.data.sceneIndex))
            {
                if (objective.data.markerTransforms[objectiveTransformIndex] != new Vector3(0, 0, 0))
                {
                    Debug.Log("moving Marker");
                    gameObject.transform.position = objective.data.markerTransforms[objectiveTransformIndex];
                }
                else
                {
                    ScreenSpaceIndicator.disableIndicator = true;
                    ScreenSpaceIndicator.disableOnScreenIndicator = true;
                    Debug.Log("No vector value given");
                    return;
                }
                
                if (objective.data.hasMarker)
                {
                    ScreenSpaceIndicator.disableOnScreenIndicator = false;
                }
                
                if (objective.data.hasOffScreenMarker)
                {
                    ScreenSpaceIndicator.disableIndicator = false;
                }
            }
            else
            {
                Debug.Log("Scenes don't match");
                ScreenSpaceIndicator.disableIndicator = true;
                ScreenSpaceIndicator.disableOnScreenIndicator = true;
                return;
            }
        }
        
    }

    private void ObjectiveProgressed(ObjectiveInstance instance)
    {
        objectiveTransformIndex++;
        if(instance.data.hasMultiplepositions && objectiveTransformIndex<= instance.data.markerTransforms.Count)
        {
            gameObject.transform.position = instance.data.markerTransforms[objectiveTransformIndex];
        }
    }

    private void OnSceneLoad()
    {
        ObjectiveInstance objective = ObjectiveManager.Instance.GetActiveObjectives().FirstOrDefault();
        
        Refresh(objective, SceneManager.GetActiveScene());

        if (WorldIndicator.GetComponent<ObjectiveSpriteBillboard>() != null)
        {
            WorldIndicator.GetComponent<ObjectiveSpriteBillboard>().FindCamera();    
        }

    }
}
