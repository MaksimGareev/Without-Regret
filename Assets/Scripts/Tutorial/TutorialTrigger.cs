using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public InteractType tutorialType;

    [Header("Options")]
    public bool triggerOnce = true;

    public ItemData requiredItem;
    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Make sure it is the player
        if (!other.CompareTag("Player")) return;

        // Stop the player from retriggering the tutorial
        if (triggerOnce && hasTriggered) return;
        
        if (requiredItem && !CheckIfPlayerHasItem()) return;

        if (InteractionTutorialUI.Instance != null)
        {
            string text = InteractionTutorialUI.Instance.GetTutorialText(tutorialType);
            InteractionTutorialUI.Instance.ShowTutorial(tutorialType, text);
        }

        // don't let trigger fire again
        hasTriggered = true;
    }

    private bool CheckIfPlayerHasItem()
    {
        if (!requiredItem) return true;
        
        Inventory playerInventory = FindAnyObjectByType<Inventory>();
        if (playerInventory)
        {
            return playerInventory.HasItemInInventory(requiredItem);
        }
        
        return false;
    }
}
