using UnityEngine;
using UnityEngine.UI;

public class WorldPopup : MonoBehaviour
{
    [Header("UI Element Reference")]
    public RectTransform uiElement;
    public Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    
    [Header("Distance Values")]
    public float minDistance = 15f;
    public float maxDistance = 40f;
    
    [Header("Scaling Values")]
    public float maxScale = 1f;
    public float minScale = 0.5f;

    [Header("Alpha Opacity Values")]
    public float maxAlpha = 1f;
    public float minAlpha = 0.2f;

    private Camera cam;
    private CanvasGroup canvasGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;

        canvasGroup = uiElement.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = uiElement.gameObject.AddComponent<CanvasGroup>();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (uiElement == null && cam == null)
        {
            return;
        }

        Vector3 worldPosition = transform.position + worldOffset;
        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);

        uiElement.position = screenPosition;

        if (screenPosition.z < 0)
        {
            uiElement.gameObject.SetActive(false);
        }
        else
        {
            uiElement.gameObject.SetActive(true);
        }

        float distance = Vector3.Distance(cam.transform.position, transform.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);

        float scale = Mathf.Lerp(maxScale, minScale, t);
        uiElement.localScale = new Vector3(scale, scale, 1);

        float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);
        canvasGroup.alpha = alpha;
    }
}
