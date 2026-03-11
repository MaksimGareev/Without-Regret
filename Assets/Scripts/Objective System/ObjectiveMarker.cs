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
            ScreenSpaceIndicator.target = gameObject.transform;
        }
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(ObjectiveCompleted);
    }

    private void DisableIndicator()
    {
        WorldIndicator.SetActive(false);
    }

    private void EnableIndicator()
    {
        WorldIndicator.SetActive(true);
    }

    private void ObjectiveCompleted(ObjectiveInstance objective)
    {
        Refresh(objective, SceneManager.GetActiveScene());
    }

    private void Refresh(ObjectiveInstance objective, Scene scene)
    {
        if (objective.data.markerTransform != null)
        {
            if (int.Equals(scene.buildIndex, objective.data.sceneIndex))
            {
                if (objective.data.markerTransform != new Vector3(0, 0, 0))
                {
                    Debug.Log("moving Marker");
                    gameObject.transform.position = objective.data.markerTransform;
                }
                else
                {
                    WorldIndicator.SetActive(false);
                    ScreenSpaceIndicator.disableIndicator = true;
                    Debug.Log("No vector value given");
                    return;
                }
                
                if (objective.data.hasMarker)
                {
                    WorldIndicator.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Scenes don't match");
                WorldIndicator.SetActive(false);
            }
        }
        else
        {
            WorldIndicator.SetActive(false);
        }
        if (objective.data.hasOffScreenMarker)
        {
            ScreenSpaceIndicator.disableIndicator = false;
        }
        else
        {
            ScreenSpaceIndicator.disableIndicator = true;
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
