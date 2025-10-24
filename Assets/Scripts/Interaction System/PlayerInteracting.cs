using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteracting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject promptUI;

    [Header("General Settings")]
    [SerializeField] private float interactionRange = 1f;
    [SerializeField] private float interactOffset = 1f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string interactButton = "Xbox X Button";

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs;
    private IInteractable currentTarget;

    private void Start()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ScanForInteractable();

        if (currentTarget != null && (Input.GetKeyDown(interactKey) || Input.GetButtonDown(interactButton)))
        {
            currentTarget.OnPlayerInteraction(gameObject);
        }
    }

    private void ScanForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * interactOffset, interactionRange);
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

            promptUI.SetActive(false);

            return;
        }
        else
        {
            currentTarget = interactableList
                .OrderByDescending(i => i.interactionPriority)
                .ThenBy(i => Vector3.Distance(transform.position, ((MonoBehaviour)i).transform.position))
                .FirstOrDefault();

            if (showDebugLogs)
            {
                Debug.Log("PlayerInteracting: Interactable object found!");
            }

            promptUI.SetActive(true);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * interactOffset, interactionRange);
    }
}
