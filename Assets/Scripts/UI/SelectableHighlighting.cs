using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectableHighlighting : MonoBehaviour
{
    private Color primaryHighlightColor = Color.aquamarine;
    private Color backgroundHighlightColor = new Color(0.25f, 0.7f, 0.6f, 1.0f);
    //private Color primaryOriginalColor;
    //private Color backgroundOriginalColor;
    private Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f); // Semi-transparent gray for disabled state
    private float highlightScaleMultiplier = 1.1f; // Scale factor for highlighting
    private ColorBlock originalColorBlock; // To store the original color block of the Selectable

    private Selectable selectable;
    private Graphic backgroundGraphic;
    private Graphic primaryGraphic;
    private Vector3 originalScale;
    [HideInInspector] public bool stayHighlighted = false;
    //private bool currentInteractable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();

        if (selectable == null)
        {
            Debug.LogError("ButtonHighlighting script must be attached to a GameObject with a Selectable component.");
            return;
        }

        SetGraphicsByType();

        if (primaryGraphic != null)
        {
            //primaryOriginalColor = primaryGraphic.color; // Store the original text color
        }

        if (backgroundGraphic != null)
        {
            //backgroundOriginalColor = backgroundGraphic.color; // Store the original background color
        }

        originalColorBlock = selectable.colors; // Store the original color block to restore later if needed
        ApplyColorOverride();

        //currentInteractable = selectable.interactable;
    }

    private void AddEventTriggers()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }
        else
        {
            trigger.triggers.Clear();
        }

        // Add pointer enter event
        EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        trigger.triggers.Add(entryEnter);

        // Add pointer exit event
        EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
        trigger.triggers.Add(entryExit);

        // Add select event
        EventTrigger.Entry entrySelect = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        entrySelect.callback.AddListener((data) => { OnSelect((BaseEventData)data); });
        trigger.triggers.Add(entrySelect);

        // Add deselect event
        EventTrigger.Entry entryDeselect = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
        entryDeselect.callback.AddListener((data) => { OnDeselect((BaseEventData)data); });
        trigger.triggers.Add(entryDeselect);
    }

    private void RemoveEventTriggers()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger != null)
        {
            trigger.triggers.Clear();
        }
    }

    private void OnEnable()
    {
        AddEventTriggers();
    }

    private void OnDisable()
    {
        RemoveEventTriggers();
        RemoveHighlight(); // Ensure highlight is removed when disabled
    }

    // Implementing the interface methods to handle pointer and selection events
    public void OnDeselect(BaseEventData eventData)
    {
        RemoveHighlight();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!selectable.interactable) return;
        ApplyHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (stayHighlighted) return; // If the button should stay highlighted, don't unhighlight it
        RemoveHighlight();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selectable.interactable) return;
        EventSystem.current.SetSelectedGameObject(null);
        ApplyHighlight();
    }

    // private void Update()
    // {
    //     if (selectable == null) return;

    //     // Check for changes in interactable state
    //     if (selectable.interactable != currentInteractable)
    //     {
    //         currentInteractable = selectable.interactable;
    //         ApplyInteractableVisuals(currentInteractable);
    //     }
    // }

    private void ApplyColorOverride()
    {
        if (selectable == null) return;

        ColorBlock colors = selectable.colors;

        colors.highlightedColor = backgroundHighlightColor;
        colors.pressedColor = backgroundHighlightColor * 0.9f;
        colors.selectedColor = backgroundHighlightColor;
        colors.disabledColor = disabledColor;
        colors.colorMultiplier = 1.0f;
        colors.fadeDuration = 0.1f;

        selectable.colors = colors;
    }

    private void ResetColorOverride()
    {
        if (selectable == null) return;

        selectable.colors = originalColorBlock; // Restore the original color block
    }

    private void SetGraphicsByType()
    {
        if (selectable == null) return;

        if (selectable is Button)
        {
            primaryGraphic = GetComponentInChildren<TextMeshProUGUI>();
            backgroundGraphic = GetComponent<Image>();
            //backgroundHighlightColor = primaryHighlightColor;
            originalScale = this.transform.localScale;
        }
        else if (selectable is Toggle toggle)
        {
            if (toggle.graphic != null)
            {
                primaryGraphic = toggle.graphic;
            }

            backgroundGraphic = toggle.targetGraphic;
            originalScale = this.transform.localScale;
        }
        else if (selectable is TMP_Dropdown dropdown)
        {
            if (dropdown.captionText != null)
            {
                primaryGraphic = dropdown.captionText;
            }

            backgroundGraphic = dropdown.targetGraphic;
            originalScale = this.transform.localScale;
        }
        else if (selectable is Slider slider)
        {
            if (slider.handleRect != null)
            {
                backgroundGraphic = slider.handleRect.GetComponent<Image>();
                originalScale = slider.handleRect.localScale;
            }

            primaryGraphic = null;
        }
        else
        {
            Debug.LogWarning("Selectable type not supported: " + selectable.GetType().Name);
        }
    }

    private void SetScaleByType(bool highlighted)
    {
        if (selectable == null) return;

        if (selectable is Button)
        {
            if (highlighted)
            {
                this.transform.localScale = originalScale * highlightScaleMultiplier;
            }
            else
            {
                this.transform.localScale = originalScale;
            }
        }
        else if (selectable is Toggle)
        {
            highlightScaleMultiplier = 1.3f;

            if (highlighted)
            {
                this.transform.localScale = originalScale * highlightScaleMultiplier;
            }
            else
            {
                this.transform.localScale = originalScale;
            }
        }
        else if (selectable is TMP_Dropdown)
        {
            highlightScaleMultiplier = 1.1f;

            if (highlighted)
            {
                this.transform.localScale = originalScale * highlightScaleMultiplier;
            }
            else
            {
                this.transform.localScale = originalScale;
            }
        }
        else if (selectable is Slider slider)
        {
            highlightScaleMultiplier = 1.25f;

            if (highlighted)
            {
                slider.handleRect.localScale = originalScale * highlightScaleMultiplier;
            }
            else
            {
                slider.handleRect.localScale = originalScale;
            }
        }
    }

    // Method to apply the highlight effect to the button
    public void ApplyHighlight(bool isTab = false)
    {
        if (selectable == null) return;

        // Change the text color to the highlight color
        if (primaryGraphic != null)
        {
            primaryGraphic.color = primaryHighlightColor;
        }

        // Change the background color to the highlight color
        if (isTab && backgroundGraphic != null)
        {
            backgroundGraphic.color = backgroundHighlightColor;
        }

        // Scale up the button slightly for a more pronounced highlight effect
        SetScaleByType(true);
    }

    // Method to reset the button's appearance to its original state
    public void RemoveHighlight(bool isTab = false)
    {
        if (selectable == null) return;

        if (stayHighlighted) stayHighlighted = false; // Reset the stayHighlighted flag when unhighlighting

        // Reset the button scale
        SetScaleByType(false);

        // if (!selectable.interactable)
        // {
        //     ApplyInteractableVisuals(false);
        //     return;
        // }

        // Reset the text color
        if (primaryGraphic != null)
        {
            primaryGraphic.color = originalColorBlock.normalColor;
        }

        // Reset the background color
        if (isTab && backgroundGraphic != null)
        {
            backgroundGraphic.color = originalColorBlock.normalColor;
        }
    }
}
