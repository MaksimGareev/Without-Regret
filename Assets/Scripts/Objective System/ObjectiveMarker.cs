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
    [SerializeField] SceneLoadManager sceneManager;

    private int objectiveTransformIndex;
    private bool markerShown = false;
    public bool MarkerShown => markerShown;

    private void OnEnable()
    {
        if (sceneManager != null) sceneManager.OnSceneLoaded.AddListener(OnSceneLoad);
    }

    private void OnDisable()
    {
        if (sceneManager != null) sceneManager.OnSceneLoaded.RemoveListener(OnSceneLoad);
    }

    private void Awake()
    {
        if (ScreenSpaceIndicator != null && WorldIndicator != null)
        {
            ScreenSpaceIndicator.target = WorldIndicator.transform;
        }
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(ObjectiveActivated);
        ObjectiveManager.Instance.OnObjectiveProgressUpdated.AddListener(ObjectiveProgressed);
    }

    private void ObjectiveActivated(ObjectiveInstance objective)
    {
        objectiveTransformIndex = 0;
        Refresh(objective, SceneManager.GetActiveScene());
    }

    private void Refresh(ObjectiveInstance objective, Scene scene)
    {
        if (!objective.data) return;
        
        if (objective.data.markerTransforms != null && objective.data.markerTransforms.Count > 0)
        {
            if (int.Equals(scene.buildIndex, objective.data.sceneIndex))
            {
                // Ensure the index is within bounds
                if (objectiveTransformIndex < objective.data.markerTransforms.Count)
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
                    Debug.Log("Marker transform index out of bounds");
                    ScreenSpaceIndicator.disableIndicator = true;
                    ScreenSpaceIndicator.disableOnScreenIndicator = true;
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
        else
        {
            // No marker transforms defined for this objective, disable indicators
            Debug.Log("No marker transforms defined for objective: " + objective.data.title);
            ScreenSpaceIndicator.disableIndicator = true;
            ScreenSpaceIndicator.disableOnScreenIndicator = true;
        }

        markerShown = !ScreenSpaceIndicator.disableOnScreenIndicator;
    }

    private void ObjectiveProgressed(ObjectiveInstance instance)
    {
        objectiveTransformIndex++;
        
        if(instance.data.hasMultiplepositions && objectiveTransformIndex <= instance.data.markerTransforms.Count)
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
