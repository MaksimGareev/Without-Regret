using UnityEngine;

public class Marker : MonoBehaviour
{
    Vector3 targetViewportPos;
    [HideInInspector] public Transform target;
    private Camera playerCam;
    [SerializeField] private GameObject onScreenIndicator;

    private void Enable()
    {
        SceneLoadManager.Instance.OnSceneLoaded.AddListener(RefreshCameraReference);
    }

    private void Disable()
    {
        SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(RefreshCameraReference);
    }

    private void Awake()
    {
        playerCam = Camera.main;
    }
    private void RefreshCameraReference()
    {
        playerCam = Camera.main;
    }

    void LateUpdate()
    {
        if(playerCam == null)
        {
            RefreshCameraReference();
        }
        if (target != null && target.gameObject.activeInHierarchy)
        {
            targetViewportPos = playerCam.WorldToViewportPoint(target.position);

            if (TargetIsVisible(targetViewportPos))
            {
                UpdateOnScreenPos(targetViewportPos);
            }
            else
            {
                onScreenIndicator.SetActive(false);
            }
        }
        else
        {
            onScreenIndicator.SetActive(false);
        }
    }

    private bool TargetIsVisible(Vector3 targetViewportPos)
    {
        return (targetViewportPos.x >= 0 && targetViewportPos.x <= 1 && targetViewportPos.y >= 0 && targetViewportPos.y <= 1 && targetViewportPos.z >= 0);
    }

    private void UpdateOnScreenPos(Vector3 targetViewportPos)
    {
        if (onScreenIndicator == null)
        {
            Debug.LogWarning("marker reference is missing in OffscreenObjectiveIndicator script. Please ensure there is a marker is attached to the script in the inspector.");
            return;
        }

        onScreenIndicator.transform.position = playerCam.WorldToScreenPoint(target.position);
        onScreenIndicator.SetActive(true);
        //float distance = Vector3.Distance(player.gameObject.transform.position, target.transform.position);
    }

    public void TurnOffMarker()
    {
        target = null;
        onScreenIndicator.SetActive(false);
    } 
}
