using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI progressText;

    private ObjectiveInstance currentObjective;

    private void OnEnable()
    {
        if(ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.AddListener(HandleObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(HandleObjectiveCompleted);
        }
    }

    private void OnDisable()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveActivated.RemoveListener(HandleObjectiveActivated);
            ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(HandleObjectiveCompleted);
        }
    }

    private void HandleObjectiveActivated(ObjectiveInstance newObjective)
    {
        Debug.Log($"[ObjectiveUI] Received objective activation: {newObjective.data.title}");
        
        currentObjective = newObjective;

        if (currentObjective == null)
        {
            Debug.Log("currentobjective is null");
        }
        RefreshUI(currentObjective);
    }

    private void HandleObjectiveCompleted(ObjectiveInstance completedObjective)
    {
        if (completedObjective == currentObjective)
        {
            currentObjective = null;
            RefreshUI(null);
        }
    }
    
    private void RefreshUI(ObjectiveInstance objective)
    {
        if (objective == null)
        {
            titleText.text = "";
            descriptionText.text = "";
            progressText.text = "";
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            titleText.text = objective.data.title;
            descriptionText.text = objective.data.description;
            progressText.text = $"{objective.currentProgress} / {objective.data.requiredProgress}";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentObjective != null)
        {
            progressText.text = $"{currentObjective.currentProgress} / {currentObjective.data.requiredProgress}";
        }
    }
}
