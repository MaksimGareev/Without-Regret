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

        ControllerButton.GetComponent<ButtonHighlighting>().stayHighlighted = true;
        ControllerButton.GetComponent<ButtonHighlighting>().OnHighlight();
        KeyboardMouseButton.GetComponent<ButtonHighlighting>().OnUnhighlight();
    }

    private void OnKeyboardMouseClicked()
    {
        controllerMapping.SetActive(false);
        mouseKeyMapping.SetActive(true);

        ControllerButton.GetComponent<ButtonHighlighting>().OnUnhighlight();
        KeyboardMouseButton.GetComponent<ButtonHighlighting>().stayHighlighted = true;
        KeyboardMouseButton.GetComponent<ButtonHighlighting>().OnHighlight();
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
