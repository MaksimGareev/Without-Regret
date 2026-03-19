using System;
using System.Collections;
using UnityEngine;

public class RemoveableObject : MonoBehaviour, IInteractable
{
    [Tooltip("Item required to remove this object. If null, no item is required and the object can be removed immediately.")]
    [SerializeField] ItemData requiredItem;
    [SerializeField] ParticleSystem removalVFX;
    public float interactionPriority => 1;
    public InteractType interactType => InteractType.Remove;
    public event Action OnInteracted;
    [SerializeField] private CleanupLeavesObjective objective;

    private MeshRenderer[] renderers;

    private bool interactable = true;

    void Start()
    {
        objective = (CleanupLeavesObjective)FindFirstObjectByType(typeof(CleanupLeavesObjective));

        renderers = GetComponentsInChildren<MeshRenderer>();
    }
    void OnEnable()
    {
        interactable = true;
    }
    void OnDisable()
    {
        interactable = false;
    }    

    public void OnPlayerInteraction(GameObject player)
    {
        if (!interactable) return;

        if (requiredItem != null && player.TryGetComponent<Inventory>(out var items))
        {
            if (!items.KeyItems.Contains(requiredItem))
            {
                // Required item is not in inventory
                Debug.Log($"Player tried to remove {gameObject.name}, but is missing required item {requiredItem.ItemName}");
                return;
            }
        }

        // Object is removed; notify listeners and disable gameobject
        OnInteracted?.Invoke();
        if (objective != null)
        {
            objective.AddLeaves();
        }
        if (removalVFX != null)
        {
            StartCoroutine(PlayVFXAndDisable());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator PlayVFXAndDisable()
    {
        // Disable renderers to hide the object while the VFX plays
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
        removalVFX.Play();
        yield return new WaitForSeconds(removalVFX.main.duration);

        gameObject.SetActive(false);
    }

    public bool CanInteract(GameObject player)
    {
        return interactable;
    }

    public void SetInteractable(bool state)
    {
        interactable = state;
    }
}
