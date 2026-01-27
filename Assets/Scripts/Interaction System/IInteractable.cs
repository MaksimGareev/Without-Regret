using UnityEngine;

public interface IInteractable
{
    float InteractionPriority { get; }
    void OnPlayerInteraction(GameObject player);
}
