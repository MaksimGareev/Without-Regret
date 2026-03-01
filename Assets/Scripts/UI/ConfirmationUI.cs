using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public enum ConfirmationType
{
    QuitToDesktop,
    QuitToMainMenu,
    ReloadSave,
    DeleteSave,
    ApplySettings,
    ResetSettings,
    DiscardChanges,
    LeaveBeforeApplyingSettings,
    FeedbackSurvey,
}

public class ConfirmationUI : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction confirmAction;
    private InputAction cancelAction;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private Button confirmButton;
    [SerializeField] public Button cancelButton;

    private bool isConfirming = false;

    private void Start()
    {
        // Initialize input actions
        confirmAction = inputActions.FindActionMap("UI").FindAction("Submit");
        if (confirmAction == null)
        {
            Debug.LogError("Confirm action not found in InputActionAsset.");
            return;
        }
        confirmAction.Enable();

        cancelAction = inputActions.FindActionMap("UI").FindAction("Cancel");
        if (cancelAction == null)
        {
            Debug.LogError("Cancel action not found in InputActionAsset.");
            return;
        }
        cancelAction.Enable();
    }

    private void Update()
    {
        if (confirmAction.triggered && isConfirming && confirmButton.interactable)
        {
            confirmButton.onClick.Invoke();
        }
        else if (cancelAction.triggered && isConfirming && cancelButton.interactable)
        {
            cancelButton.onClick.Invoke();
        }
    }

    private void UpdateTaskText(ConfirmationType type)
    {
        switch (type)
        {
            case ConfirmationType.QuitToDesktop:
                taskText.text = "quit to desktop?";
                break;
            case ConfirmationType.QuitToMainMenu:
                taskText.text = "quit to the main menu?\n\nYour progress will be saved.";
                break;
            case ConfirmationType.ReloadSave:
                taskText.text = "reload the last save?\n\nUnsaved progress will be lost.";
                break;
            case ConfirmationType.DeleteSave:
                taskText.text = "delete this save?\n\nThis action cannot be undone.";
                break;
            case ConfirmationType.ApplySettings:
                taskText.text = "apply these settings?";
                break;
            case ConfirmationType.ResetSettings:
                taskText.text = "reset all settings to default?";
                break;
            case ConfirmationType.DiscardChanges:
                taskText.text = "discard all changes?";
                break;
            case ConfirmationType.LeaveBeforeApplyingSettings:
                taskText.text = "exit without applying changes?\n\nYour changes will be discarded.";
                break;
            case ConfirmationType.FeedbackSurvey:
                taskText.text = "open the feedback survey in your web browser?\n\nThe game will remain open in the background.";
                break;
            default:
                taskText.text = "to proceed?";
                break;
        }
    }

    public void ConfirmTask(ConfirmationType type, System.Action onConfirm, System.Action onCancel)
    {
        isConfirming = true;
        UpdateTaskText(type);

        StartCoroutine(WaitBeforeInput(onConfirm, onCancel));

        EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
    }

    private IEnumerator WaitBeforeInput(System.Action onConfirm = null, System.Action onCancel = null)
    {
        yield return new WaitForSecondsRealtime(0.2f);

        if (onConfirm != null)
        {
            confirmButton.onClick.AddListener(() => onConfirm.Invoke());
            confirmButton.interactable = true;
        }
        if (onCancel != null)
        {
            cancelButton.onClick.AddListener(() => onCancel.Invoke());
            cancelButton.interactable = true;
        }
    }

    public void EndConfirmation()
    {
        isConfirming = false;

        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        confirmButton.interactable = false;
        cancelButton.interactable = false;

        confirmButton.GetComponent<SelectableHighlighting>().RemoveHighlight();
        cancelButton.GetComponent<SelectableHighlighting>().RemoveHighlight();

        StopAllCoroutines();
    }

    private void OnDisable()
    {
        EndConfirmation();
    }
}
