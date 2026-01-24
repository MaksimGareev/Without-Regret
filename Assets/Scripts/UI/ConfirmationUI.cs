using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum ConfirmationType
{
    QuitToDesktop,
    QuitToMainMenu,
    ReloadSave,
    DeleteSave,
    ApplySettings,
    ResetSettings,
    DiscardChanges
}

public class ConfirmationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

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
            default:
                taskText.text = "to proceed?";
                break;
        }
    }

    public void ConfirmTask(ConfirmationType type, System.Action onConfirm, System.Action onCancel)
    {
        UpdateTaskText(type);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => onConfirm.Invoke());

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => onCancel.Invoke());
    }
}
