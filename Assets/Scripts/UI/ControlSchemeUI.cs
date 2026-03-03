using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class ControlSchemeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Button ControllerButton;
    [SerializeField] private Button KeyboardMouseButton;
    [SerializeField] private GameObject controllerMapping;
    [SerializeField] private GameObject mouseKeyMapping;
    [SerializeField] private GameObject controllerLegends;
    [SerializeField] private GameObject mouseKeyLegends;
    private bool usingController = true;
    
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

    private void Update()
    {
        CheckMouseInput();
        CheckControllerInput();
    }

    private void CheckMouseInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        bool mouseKeysMoved = mouseDelta.sqrMagnitude > 0.1f || Keyboard.current.anyKey.isPressed;

        if (!mouseKeysMoved) return;
        
        if (usingController)
        {
            usingController = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            if (controllerLegends.activeSelf)
            {
                controllerLegends.SetActive(false);
            }

            if (!mouseKeyLegends.activeSelf)
            {
                mouseKeyLegends.SetActive(true);
            }
        }
    }

    private void CheckControllerInput()
    {
        if (Gamepad.current == null)
        {
            return;
        }

        bool controllerMoved = 
            Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f 
            || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f
            || Gamepad.current.leftShoulder.IsPressed() 
            || Gamepad.current.rightShoulder.IsPressed();
        
        if (!controllerMoved)
        {
            return;
        }

        if (!usingController)
        {
            usingController = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            var es = EventSystem.current;

            // Clear selected GameObject if mouse was hovering over something
            if (es.IsPointerOverGameObject())
            {
                var ped = new PointerEventData(es)
                {
                    position = new Vector2(-99999f, -99999f)
                };

                es.RaycastAll(ped, new System.Collections.Generic.List<RaycastResult>());
                es.SetSelectedGameObject(null);

                InputSystemUIInputModule inputModule = es.currentInputModule as InputSystemUIInputModule;
                if (inputModule != null)
                {
                    inputModule.enabled = false;
                    inputModule.enabled = true;
                }
            }

            // If nothing is selected, set a default based on the active panel
            if (es.currentSelectedGameObject == null)
            {
                if (GetComponentInParent<MMSettings>()?.GetComponentInParent<MainMenu>() != null)
                {
                    es.SetSelectedGameObject(GetComponentInParent<MMSettings>()?.GetComponentInParent<MainMenu>()?.backButton.gameObject);
                }
                else if (GetComponentInParent<MMSettings>()?.GetComponentInParent<PauseManager>() != null)
                {
                    es.SetSelectedGameObject(GetComponentInParent<MMSettings>()?.GetComponentInParent<PauseManager>()?.backButton.gameObject);
                }
            }

            if (!controllerLegends.activeSelf)
            {
                controllerLegends.SetActive(true);
            }

            if (mouseKeyLegends.activeSelf)
            {
                mouseKeyLegends.SetActive(false);
            }
        } 
    }

    private void OnControllerClicked()
    {
        controllerMapping.SetActive(true);
        mouseKeyMapping.SetActive(false);

        ControllerButton.GetComponent<SelectableHighlighting>().stayHighlighted = true;
        ControllerButton.GetComponent<SelectableHighlighting>().ApplyHighlight(true);
        KeyboardMouseButton.GetComponent<SelectableHighlighting>().RemoveHighlight(true);
    }

    private void OnKeyboardMouseClicked()
    {
        controllerMapping.SetActive(false);
        mouseKeyMapping.SetActive(true);

        ControllerButton.GetComponent<SelectableHighlighting>().RemoveHighlight(true);
        KeyboardMouseButton.GetComponent<SelectableHighlighting>().stayHighlighted = true;
        KeyboardMouseButton.GetComponent<SelectableHighlighting>().ApplyHighlight(true);
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
