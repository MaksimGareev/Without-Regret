using UnityEngine;

public interface IInteractable
{
    float interactionPriority { get; }
    void OnPlayerInteraction(GameObject player);
}
