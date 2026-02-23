using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteracting : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private InputActionAsset InputActions;
    [SerializeField] private float interactionRange = 3f;

    private InputAction Interact;
    private InputAction Mantle;

    private List<IInteractable> currentTargets = new List<IInteractable>();

    private bool currentlyInteracting = false;

    private float lastDetectionTime;
    [SerializeField] private float detectionGraceTime = 0.15f;

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
        // Stop interaction during tutorial UI
        if (InteractionTutorialUI.Instance != null &&
            InteractionTutorialUI.Instance.IsShowing)
        {
            return;
        }

        // If holding an object press interact again to realease it (stop picking up multiple items)
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

        if (currentTargets.Count == 0)
            return;
        

        // Interact with mantleable Objects
        if (Mantle.triggered)
        {
            var mantle = currentTargets.FirstOrDefault(i => i.interactType == InteractType.Mantle);

            if (mantle != null)
            {
                mantle.OnPlayerInteraction(gameObject);
                return;
            }
        }

        // Interact with moveable objects or dialogue
        if (Interact.triggered)
        {
            var interact = currentTargets.FirstOrDefault(i => i.interactType != InteractType.Mantle);

            if (interact != null)
            {
                HandleInteracton(interact);
            }
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

    // Scan for interactable objects or triggers in front of the player
    private void ScanForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange,~0, QueryTriggerInteraction.Collide);

        var detectedTargets = hits
                                .SelectMany(h => h.GetComponentsInParent<IInteractable>())
                                .Distinct().Where(i => i != null && i.CanInteract(gameObject))
                                .OrderByDescending(i => i.interactionPriority)
                                .ToList();
        
        // Check if something is detected in this frame and highlight interaction button
        if (detectedTargets.Count > 0)
        {
            currentTargets = detectedTargets;
            lastDetectionTime = Time.time;

            ButtonIcons.Instance?.HighlightMultiple(currentTargets);
            return;
        }

        // if nothing is detected clear the buttons after the detection grace period and clear interaction buttons
        if (Time.time - lastDetectionTime > detectionGraceTime)
        {
            currentTargets.Clear();
            ButtonIcons.Instance?.Clear();
        }

    }

    // Activate tutorial description on first interaction with specific interaction type
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
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}