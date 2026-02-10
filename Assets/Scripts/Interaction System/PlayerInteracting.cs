using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteracting : MonoBehaviour
{
    [Header("References")]
    //[SerializeField] private GameObject promptUI;

    [Header("General Settings")]
    [SerializeField] private InputActionAsset InputActions;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float interactOffset = 1f;
    
    private InputAction Interact;
    private InputAction Mantle;
    private bool currentlyInteracting = false;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs;
    private IInteractable currentTarget;
    private MantleableObject mantleTarget;
    private MoveableObject moveTarget;

    // Moveable object
    private MoveableObject heldObject;

    private void Awake()
    {
        Mantle = InputActions.FindActionMap("Player").FindAction("Jump");
        Interact = InputActions.FindActionMap("Player").FindAction("Interact");
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
        if (InteractionTutorialUI.Instance != null &&
            InteractionTutorialUI.Instance.IsShowing)
        {
            return;
        }

        if (heldObject != null)
        {
            if (Interact.triggered)
            {
                //TryShowTutorial(heldObject.interactType);
                heldObject.OnPlayerInteraction(gameObject);
            }
            return;
        }

        ScanForInteractable();

        if (currentTarget == null)
            return;

        var targetMono = currentTarget as MonoBehaviour;
        if (targetMono == null)
            return;

        // Interact with mantleable Objects
        if (mantleTarget != null && Mantle != null && Mantle.triggered)
        {
            if (showDebugLogs) Debug.Log("Mantle input detected!");
            //TryShowTutorial(mantleTarget.interactType);
            mantleTarget.OnPlayerInteraction(gameObject);
            return;
        }

        // Interact with moveable objects or dialogue
        if (currentTarget != null && Interact != null && Interact.triggered)
        {
            //TryShowTutorial(currentTarget.interactType);
            //currentTarget.OnPlayerInteraction(gameObject);
            HandleInteracton(currentTarget);
        }

    }

    public void HandleInteracton(IInteractable target)
    {
        if (target == null)
            return;

        bool tutorialShown = TryShowTutorial(target.interactType, () => target.OnPlayerInteraction(gameObject));

        if (tutorialShown)
            return;

        target.OnPlayerInteraction(gameObject);
    }

    public void SetHeldObject(MoveableObject obj)
    {
        heldObject = obj;
    }

    public void ClearHeldObjects()
    {
        heldObject = null;
    }

    private void ScanForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);

        var interactables = hits.Select(h => h.GetComponent<IInteractable>()).Where(i => i != null && i.CanInteract(gameObject)).ToList();  //OrderByDescending(i => i.interactionPriority).FirstOrDefault();

        if (interactables.Count == 0)
        {
            currentTarget = null;
            mantleTarget = null;
            moveTarget = null;
            ButtonIcons.Instance?.Clear();
            return;
        }

        currentTarget = interactables.OrderByDescending(i => i.interactionPriority).FirstOrDefault();

        mantleTarget = currentTarget as MantleableObject;
        moveTarget = currentTarget as MoveableObject;

        // Highlight icon
        ButtonIcons.Instance?.Highlight(currentTarget.interactType);

       
    }
    
    private bool TryShowTutorial(InteractType type, System.Action onComplete)
    {
        if (InteractionTutorialManager.Instance == null)
            return false;

        if (InteractionTutorialManager.Instance.HasSeenTutorial(type))
            return false;

        if (InteractionTutorialUI.Instance == null)
        {
            Debug.LogError("TutorialUI missing");
            return false;
        }

        string tutorialText = InteractionTutorialText.GetText(type);

        if (string.IsNullOrEmpty(tutorialText))
            return false;

        InteractionTutorialUI.Instance.ShowTutorial(tutorialText, () =>
        {
            InteractionTutorialManager.Instance.MarkTutorialSeen(type);
            onComplete?.Invoke();
        }
        );

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * interactOffset, interactionRange);
    }
}
