using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ChimeHintUI : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction hintAction;
    public GameObject hintBubbleUI;
    public TextMeshProUGUI hintText;
    private Transform cam;

    public float displayTime = 3f;

    private bool isShowing = false;
    private Coroutine hideRoutine;

    [Header("Animation settings")]
    public Animator animator;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (hintBubbleUI != null)
        {
            hintBubbleUI.SetActive(false);
        }
        cam = Camera.main.transform;

        // Setup input action
        hintAction = inputActions.FindAction("Player/ChimeHint");
        hintAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (hintAction?.triggered ?? false)
        {
            ShowHint();
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

    public void ShowHint()
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
    }

    private IEnumerator HideBubbleAfterDelay()
    {
        animator.SetBool("isHinting", false);

        yield return new WaitForSeconds(displayTime);
        hintBubbleUI.SetActive(false);
        hideRoutine = null;
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
