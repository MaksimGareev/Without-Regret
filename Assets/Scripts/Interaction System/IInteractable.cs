using UnityEngine;

public interface IInteractable
{
    float interactionPriority { get; }
    InteractType interactType { get; }
    void OnPlayerInteraction(GameObject player);
}
