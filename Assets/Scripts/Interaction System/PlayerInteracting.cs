using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteracting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject promptUI;

    [Header("General Settings")]
    [SerializeField] private InputActionAsset InputActions;
    [SerializeField] private float interactionRange = 1f;
    [SerializeField] private float interactOffset = 1f;
    private InputAction Interact;
    private InputAction Mantle;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs;
    private IInteractable currentTarget;
    private MantleableObject mantleTarget;
    private MoveableObject moveTarget;

    private void Awake()
    {
        Mantle = InputActions.FindActionMap("Player").FindAction("Jump");
        Interact = InputActions.FindActionMap("Player").FindAction("Interact");
    }


    private void Start()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Mantle.Enable();
        Interact.Enable();
    }

    private void OnDisable()
    {
        Mantle.Disable();
        Interact.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        ScanForInteractable();

        if (mantleTarget != null && (Mantle.triggered))
        {
            mantleTarget.OnPlayerInteraction(gameObject);
        }
        if (moveTarget != null && (Interact.triggered))
        {
            moveTarget.OnPlayerInteraction(gameObject);
        }
        else if (currentTarget != null && mantleTarget == null && (Interact.triggered))
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
            mantleTarget = null;
            moveTarget = null;

            if (showDebugLogs)
            {
                Debug.Log("PlayerInteracting: No interactables found.");
            }

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            return;
        }

        currentTarget = interactableList
            .OrderByDescending(i => i.interactionPriority)
            .ThenBy(i => Vector3.Distance(transform.position, ((MonoBehaviour)i).transform.position))
            .FirstOrDefault();

        if(currentTarget != null)
        {
            var targetMono = (currentTarget as MonoBehaviour);

            mantleTarget = targetMono.GetComponent<MantleableObject>();
            moveTarget = targetMono.GetComponent<MoveableObject>();

            if (showDebugLogs)
            {
                Debug.Log("PlayerInteracting: Interactable object found!");
            }

            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * interactOffset, interactionRange);
    }
}
