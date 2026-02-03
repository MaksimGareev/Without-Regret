using UnityEngine;

public interface IInteractable
{
    float interactionPriority { get; }
    InteractType interactType { get; }

    bool CanInteract(GameObject player);
    void OnPlayerInteraction(GameObject player);
}
