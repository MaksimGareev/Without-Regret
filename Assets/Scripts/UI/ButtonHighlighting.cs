using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlighting : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Style Settings")]
    [Tooltip("The original color of the button's text.")]
    [SerializeField] private static Color textOriginalColor;

    [Tooltip("The highlight color of the button's text.")]
    [SerializeField] private static Color textHighlightColor = Color.aquamarine;

    [Tooltip("The original color of the button's background.")]
    [SerializeField] private static Color backgroundOriginalColor = Color.white;

    [Tooltip("The highlight color of the button's background.")]
    [SerializeField] private static Color backgroundHighlightColor = Color.aquamarine;
    private Button button;
    private TextMeshProUGUI textComponent;
    private Image backgroundImage;
    private Vector3 originalScale;
    [HideInInspector] public bool stayHighlighted = false;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("ButtonHighlighting script must be attached to a GameObject with a Button component.");
            return;
        }

        // Set the original colors
        textComponent = button.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textOriginalColor = textComponent.color; // Store the original text color
        }

        backgroundImage = button.GetComponent<Image>();

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundOriginalColor; // Ensure the background starts with the original color
        }

        originalScale = this.transform.localScale;  // Store the original scale of the button

        button.onClick.AddListener(OnHighlight); // Highlight the button when it's clicked
    }

    // Implementing the interface methods to handle pointer and selection events
    public void OnDeselect(BaseEventData eventData)
    {
        OnUnhighlight();
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (stayHighlighted) return; // If the button should stay highlighted, don't unhighlight it
        OnUnhighlight();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHighlight();
    }

    // Method to apply the highlight effect to the button
    public void OnHighlight()
    {
        if (button == null) return;

        // Change the text color to the highlight color
        if (textComponent != null)
        {
            textComponent.color = textHighlightColor;
        }

        // Change the background color to the highlight color
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundHighlightColor;
        }

        // Scale up the button slightly for a more pronounced highlight effect
        this.transform.localScale = originalScale * 1.1f;
    }

    // Method to reset the button's appearance to its original state
    public void OnUnhighlight()
    {
        if (button == null) return;

        if (stayHighlighted) stayHighlighted = false; // Reset the stayHighlighted flag when unhighlighting

        // Reset the text color
        if (textComponent != null)
        {
            textComponent.color = textOriginalColor;
        }

        // Reset the background color
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundOriginalColor;
        }

        // Reset the button scale
        this.transform.localScale = originalScale;
    }
}
