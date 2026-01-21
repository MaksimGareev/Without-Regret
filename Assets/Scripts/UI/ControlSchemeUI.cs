using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlSchemeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button ControllerButton;
    [SerializeField] private Button KeyboardMouseButton;
    [SerializeField] private TextMeshProUGUI controllerText;
    [SerializeField] private TextMeshProUGUI keyboardMouseText;
    
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

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnControllerClicked()
    {
        controllerText.gameObject.SetActive(true);
        keyboardMouseText.gameObject.SetActive(false);
    }

    private void OnKeyboardMouseClicked()
    {
        controllerText.gameObject.SetActive(false);
        keyboardMouseText.gameObject.SetActive(true);
    }
}
