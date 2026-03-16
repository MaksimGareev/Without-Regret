using UnityEngine;
using UnityEngine.UI;

public class OffscreenObjectiveIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("Publicly accessible value that can be set by other scripts for what the arrow should be pointing at")]
    public Transform target;
    [Tooltip("Simply the reference for the arrow indicator")]
    [SerializeField] private GameObject indicator;
    [Tooltip("Reference to the indicator when the objective is on screen")]
    [SerializeField] private GameObject onScreenIndicator;
    [Tooltip("Main Canvas which should have the arrow on it")]
    [SerializeField] private Canvas mainCanvas;
    [Tooltip("Margin of how far away the indicator will stay from the edges of the screen")]
    [SerializeField] private float offScreenMargin = 45f;
    [Tooltip("The distance limit where the arrow will sit at max size. Any further than this and the arrow stays the same, closer this and the arrow starts to shrink")]
    [SerializeField] private float maxSizeDistance = 2000f;
    [Tooltip("The distance limit where the arrow wont get any smaller. Any closer than this wont change the arrow, any farther and the arrow starts to grow again.")]
    [SerializeField] private float minSizeDistance = 20f;
    [Tooltip("Minimum size the arrow will shrink to.")]
    [SerializeField] private float minImageScale = 0.2f;
    [Tooltip("controlls whether the offscreen arrow appears or not.")]
    public bool disableIndicator = false;
    [Tooltip("controls whether the onscreen diamond appears or not.")]
    public bool disableOnScreenIndicator = false;


    private Image image;
    private float spriteWidth;
    private float spriteHeight;
    private RectTransform canvasRect, rectTransform, onScreenRectTransform;
    
    private PlayerController player;
    private float lerpAmount = 1f;
    
    private Camera playerCam;

    Vector3 screenCenter = Vector3.zero;
    Vector3 originalScale;
    [SerializeField] Vector2 localPoint;
    [SerializeField] Vector3 targetViewportPos;

    Vector3 ScreenCenter
    {
        get
        {
            var rect = canvasRect.rect;
            screenCenter.x = rect.width * 0.5f;
            screenCenter.y = rect.height * 0.5f;
            screenCenter.z = 0f;
            return screenCenter * canvasRect.localScale.x;
        }
    }

    private void Enable()
    {
        SceneLoadManager.Instance.OnSceneLoaded.AddListener(RefreshCameraReference);
    }

    private void Disable()
    {
        SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(RefreshCameraReference);
    }

    private void RefreshCameraReference()
    {
        playerCam = Camera.main;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    private void Start()
    {
        //playerCam = Camera.main;
        image = indicator.GetComponent<Image>();

        var bounds = image.sprite.bounds;
        spriteHeight = bounds.size.y / 2f;
        spriteWidth = bounds.size.x / 2f;
        indicator.SetActive(false);
        onScreenIndicator.SetActive(false);
        canvasRect = mainCanvas.GetComponent<RectTransform>();

        rectTransform = indicator.GetComponent<RectTransform>();
        onScreenRectTransform = onScreenIndicator.GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    private void LateUpdate()
    {
        if (target != null && playerCam != null && player != null)
        {
            targetViewportPos = playerCam.WorldToViewportPoint(target.position);

            if (!TargetIsVisible(targetViewportPos) && !disableIndicator)
            {
                UpdateOffscreenTarget(targetViewportPos);
                onScreenIndicator.SetActive(false);
            }
            else if(TargetIsVisible(targetViewportPos) && !disableOnScreenIndicator)
            {
                UpdateOnScreenPos(targetViewportPos);
                indicator.SetActive(false);
            }
            if (disableIndicator)
            {
                indicator.SetActive(false);
            }
            if (disableOnScreenIndicator)
            {
                onScreenIndicator.SetActive(false);
            }
            
        }
        else if (playerCam == null)
        {
            Debug.LogWarning("Player Camera reference is missing in OffscreenObjectiveIndicator script. Please ensure there is a camera in the scene tagged as 'MainCamera'.");
            RefreshCameraReference();
        }
        else if (target == null)
        {
            Debug.LogWarning("Target reference is missing in OffscreenObjectiveIndicator script. Please ensure the target variable is set to the transform of the objective you want to point towards.");
        }

    }

    private bool TargetIsVisible(Vector3 targetViewportPos)
    {
        return (targetViewportPos.x >= 0 && targetViewportPos.x <= 1 && targetViewportPos.y >= 0 && targetViewportPos.y <= 1 && targetViewportPos.z >= 0);
    }

    private void UpdateOffscreenTarget(Vector3 targetViewportPos)
    {
        if (playerCam == null)
        {
            Debug.LogWarning("Player Camera reference is missing in OffscreenObjectiveIndicator script. Please ensure there is a camera in the scene tagged as 'MainCamera'.");
            return;
        }

        Vector3 indicatorPosition = (playerCam.ViewportToScreenPoint(targetViewportPos) - ScreenCenter) 
            * Mathf.Sign(targetViewportPos.z);

        indicatorPosition.z = 0;

        float x = (ScreenCenter.x - offScreenMargin) / Mathf.Abs(indicatorPosition.x);
        float y = (ScreenCenter.y - offScreenMargin) / Mathf.Abs(indicatorPosition.y);

        if (x < y)
        {
            float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
            indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (screenCenter.x - offScreenMargin) *
                canvasRect.localScale.x;

            indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
        }
        else
        {
            float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);
            indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (screenCenter.y - offScreenMargin) *
                canvasRect.localScale.y;
            indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
        }

        indicatorPosition += ScreenCenter;
        rectTransform.position = indicatorPosition;

        Vector3 rotation = rectTransform.eulerAngles;
        rotation.z = Vector3.SignedAngle(Vector3.up, indicatorPosition - ScreenCenter, Vector3.forward);
        rectTransform.eulerAngles = rotation;

        
        float distance = Vector3.Distance(player.gameObject.transform.position, target.transform.position);

        if(distance >= maxSizeDistance)
        {
            lerpAmount = 1;
        }
        if(distance <= minSizeDistance)
        {
            lerpAmount = 0;
        }
        else
        {
            lerpAmount = ((distance - minSizeDistance) / (maxSizeDistance - minSizeDistance)) * 100;
        }

        float scale = Mathf.Lerp(minImageScale, 1, lerpAmount);

        rectTransform.localScale = originalScale * scale;
        indicator.SetActive(true);

    }

    private void UpdateOnScreenPos(Vector3 targetViewportPos)
    {
        if (playerCam == null)
        {
            Debug.LogWarning("Player Camera reference is missing in OffscreenObjectiveIndicator script. Please ensure there is a camera in the scene tagged as 'MainCamera'.");
            return;
        }

        onScreenIndicator.transform.position = playerCam.WorldToScreenPoint(target.position);
        onScreenIndicator.SetActive(true);
        //float distance = Vector3.Distance(player.gameObject.transform.position, target.transform.position);
    }
}
