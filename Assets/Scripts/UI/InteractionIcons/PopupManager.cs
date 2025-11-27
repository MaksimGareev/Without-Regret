using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;
    [SerializeField] private RectTransform uiCanvas;
    
    void Awake()
    {
        Instance = this;
    }

    public RectTransform CreatePopup(Transform worldTarget, GameObject iconPrefab)
    {
        GameObject popupInstance = Instantiate(iconPrefab, uiCanvas);
        RectTransform rect = popupInstance.GetComponent<RectTransform>();

        var wp = worldTarget.gameObject.AddComponent<WorldPopup>();
        wp.uiElement = rect;

        return rect;
    }
}
