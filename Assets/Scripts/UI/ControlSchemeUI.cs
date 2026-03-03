using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlSchemeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Button ControllerButton;
    [SerializeField] private Button KeyboardMouseButton;
    [SerializeField] private GameObject controllerMapping;
    [SerializeField] private GameObject mouseKeyMapping;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ControllerButton.onClick.AddListener(OnControllerClicked);
        KeyboardMouseButton.onClick.AddListener(OnKeyboardMouseClicked);
    }

    private void OnEnable()
    {
        OnControllerClicked();
    }

    private void OnControllerClicked()
    {
        controllerMapping.SetActive(true);
        mouseKeyMapping.SetActive(false);

        ControllerButton.GetComponent<SelectableHighlighting>().stayHighlighted = true;
        ControllerButton.GetComponent<SelectableHighlighting>().ApplyHighlight();
        KeyboardMouseButton.GetComponent<SelectableHighlighting>().RemoveHighlight();
    }

    private void OnKeyboardMouseClicked()
    {
        controllerMapping.SetActive(false);
        mouseKeyMapping.SetActive(true);

        ControllerButton.GetComponent<SelectableHighlighting>().RemoveHighlight();
        KeyboardMouseButton.GetComponent<SelectableHighlighting>().stayHighlighted = true;
        KeyboardMouseButton.GetComponent<SelectableHighlighting>().ApplyHighlight();
    }

    public void SwitchTabs()
    {
        if (controllerMapping.activeSelf)
        {
            OnKeyboardMouseClicked();
        }
        else
        {
            OnControllerClicked();
        }
    }
}
