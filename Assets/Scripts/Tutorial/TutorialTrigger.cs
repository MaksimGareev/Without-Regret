using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public InteractType tutorialType;

    [Header("Options")]
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Make sure it is the player
        if (!other.CompareTag("Player")) return;

        // Stop the player from retriggering the tutorial
        if (triggerOnce && hasTriggered) return;

        if (InteractionTutorialUI.Instance != null)
        {
            string text = InteractionTutorialUI.Instance.GetTutorialText(tutorialType);
            InteractionTutorialUI.Instance.ShowTutorial(tutorialType, text);
        }

        // don,t let trigger fire again
        hasTriggered = true;
    }
}
