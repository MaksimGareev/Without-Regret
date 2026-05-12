using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropdownAutoScroll : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private float scrollSpeed = 15f;
    
    private ScrollRect scrollRect;
    private RectTransform content;
    GameObject lastSelected;

    private void Reset()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void LateUpdate()
    {
        if (!scrollRect)
        {
            GameObject dropdownList = dropdown?.transform.Find("Dropdown List")?.gameObject;
            
            if (!dropdownList) return;
            
            scrollRect = dropdownList.GetComponentInChildren<ScrollRect>();
            
            if (!scrollRect) return;
            
            content = scrollRect.content;
        }

        if (!scrollRect.gameObject.activeInHierarchy)
        {
            scrollRect = null;
            content = null;
            return;
        }
        
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        
        if (selected && lastSelected != selected)
        {
            lastSelected = selected;
            EventSystem.current.firstSelectedGameObject = lastSelected;
        }

        if (!selected) return;
        
        if (!selected.transform.IsChildOf(content)) return;
        
        RectTransform selectedRect = selected.GetComponent<RectTransform>();
        
        if (InputDeviceManager.Instance.CurrentMode == InputDeviceManager.InputMode.KeyboardMouse) return;
        
        ScrollToSelected(selectedRect);
    }

    private void ScrollToSelected(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();
        
        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        
        float targetY = Mathf.Abs(target.anchoredPosition.y);
        float centeredY = targetY - (viewportHeight * 0.5f);
        
        float normalized = Mathf.Clamp01(centeredY / (contentHeight - viewportHeight));
        
        scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, normalized, scrollSpeed * Time.unscaledDeltaTime);
    }
}
