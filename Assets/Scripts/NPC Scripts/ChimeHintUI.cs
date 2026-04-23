using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class ChimeHintUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayTime = 3f;
    [Tooltip("How far the player must be from the objective marker for the hint UI to pop up automatically")]
    [SerializeField] private float markerDistanceThreshold = 50f;
    [Tooltip("If a hint is shown automatically due to distance, it will only happen again after this many seconds")]
    [SerializeField] private float autoHintCooldown = 10f;

    [Header("References")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject hintBubbleUI;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Transform player;

    [Header("Animation Settings")]
    public Animator animator;

    private InputAction hintAction;
    private Transform cam;
    //private bool isShowing = false;
    private Coroutine hideRoutine;
    private Coroutine autoRoutine;
    private Transform objectiveMarker;
    private ObjectiveMarker markerComponent;
    private bool canShowAutoHint = true;

    private void Awake()
    {
        if (player == null)
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else Debug.LogError("ChimeHintUI: Player not found in scene. Chime will not function without player reference.", this);
        }
    }

    void Start()
    {
        if (hintBubbleUI != null)
        {
            hintBubbleUI.SetActive(false);
        }
        cam = Camera.main.transform;

        if (GameManager.Instance != null)
        {
            markerComponent = GameManager.Instance.ObjectiveMarker;
            objectiveMarker = markerComponent.WorldIndicator.transform;
        }

        // Setup input action
        hintAction = inputActions.FindAction("Player/ChimeHint");
        hintAction.Enable();
    }

    void Update()
    {
        if (objectiveMarker == null || player == null) return;

       /* if (hintAction?.triggered ?? false)
        {
            ShowHint();
        }*/

        if (canShowAutoHint && markerComponent.MarkerShown)
        {
            // If the player strays too far from the objective marker, show a hint
            float distance = Vector3.Distance(player.position, objectiveMarker.position);
            if (distance >= markerDistanceThreshold)
            {
                canShowAutoHint = false;
                ShowHint(startAutoCooldown: true);
            }
        }
    }

    private void LateUpdate()
    {
        if (cam == null || hintBubbleUI == null) 
        {
            return;
        }

        Vector3 camPos = cam.position;
        camPos.y = hintBubbleUI.transform.position.y;

        hintBubbleUI.transform.LookAt(cam);
        hintBubbleUI.transform.Rotate(0, 180f, 0);
        
        /*
        Vector3 lookPos = transform.position + cam.forward;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);*/
    }

    public void ShowHint(bool startAutoCooldown = false)
    {
        animator.SetBool("isHinting", true);

        // Get the current objective from Objective Manager
        ObjectiveInstance activeObjective = GetCurrentObjective();

        if (activeObjective == null)
        {
            hintText.text = ("I don't think there is anthing to do right now, best to keep exploring.");
        }
        else
        {
            hintText.text = activeObjective.data.chimeDialogue;
        }

        // Show UI
        hintBubbleUI.SetActive(true);

        // Reset timer if already counting down
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        hideRoutine = StartCoroutine(HideBubbleAfterDelay());

        if (startAutoCooldown)
        {
            if (autoRoutine != null)
                StopCoroutine(autoRoutine);

            autoRoutine = StartCoroutine(AutoHintCooldown());
        }
    }

    private IEnumerator HideBubbleAfterDelay()
    {
        animator.SetBool("isHinting", false);

        yield return new WaitForSeconds(displayTime);
        hintBubbleUI.SetActive(false);
        hideRoutine = null;
    }

    private IEnumerator AutoHintCooldown()
    {
        canShowAutoHint = false;

        yield return new WaitForSeconds(autoHintCooldown);

        canShowAutoHint = true;
        autoRoutine = null;
    }

    private ObjectiveInstance GetCurrentObjective()
    {
        foreach (var obj in ObjectiveManager.Instance.GetActiveObjectives())
        {
            return obj;
        }

        return null;
    }

    public void ShowHintMessage(string message)
    {
        hintText.text = message;
        hintBubbleUI.SetActive(true);

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        hideRoutine = StartCoroutine(HideBubbleAfterDelay());
    }
}
