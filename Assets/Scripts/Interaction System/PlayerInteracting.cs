using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteracting : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string interactButton = "Xbox X Button";

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs;
    private IInteractable currentTarget;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(interactKey) || Input.GetButtonDown(interactButton))
        {
            ScanForInteractable();
        }
    }

    private void ScanForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);
        List<IInteractable> interactableList = new List<IInteractable>();

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactableList.Add(interactable);
            }
        }

        if (interactableList.Count == 0)
        {
            currentTarget = null;

            if (showDebugLogs)
            {
                Debug.Log("PlayerInteracting: No interactables found.");
            }

            return;
        }
        else
        {
            currentTarget = interactableList
                .OrderByDescending(i => i.interactionPriority)
                .ThenBy(i => Vector3.Distance(transform.position, ((MonoBehaviour)i).transform.position))
                .FirstOrDefault();

            currentTarget.OnPlayerInteraction(gameObject);

            if (showDebugLogs)
            {
                Debug.Log("PlayerInteracting: Interactable object found!");
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
