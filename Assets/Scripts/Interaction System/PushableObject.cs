using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// When the player interacts with this object, a button mash minigame will start
// Mashing the button quick enough will tilt the object upward until it's fully upright
// The object will slowly tilt downward while the minigame is active
[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour, IInteractable
{
    [Header("Input")]
    [Tooltip("The input used by the player to mash")]
    [SerializeField] private InputActionReference mashInput;
    [Tooltip("The input used to immediately quit the minigame")]
    [SerializeField] private InputActionReference quitInput;

    [Header("Transform Reference (must be assigned)")]
    [Tooltip("The target transform used as a reference for what the end goal should be")]
    [SerializeField] private Transform referenceTransform;
    [SerializeField] private bool disableReferenceObject = true;

    [Header("Settings")]
    [Tooltip("How much progress one mash step will add.")]
    [SerializeField] private float mashProgressPerPress = 0.12f;
    [Tooltip("How much progress will passively be removed every second")]
    [SerializeField] private float passiveDecay = 0.3f;
    [Tooltip("If true the player will be frozen while the minigame runs (recommended).")]
    [SerializeField] private bool freezePlayerDuringMinigame = true;
    [Tooltip("The angle in degrees the object will rotate around the axis derived from the reference transform.")]
    [SerializeField] private float rotationAngle = 90f;
    [SerializeField, Tooltip("Pulse scale for button when mashed")] private float pulseScale = 1.18f;
    [SerializeField, Tooltip("Pulse duration in seconds")] private float pulseDuration = 0.12f;

    [Header("UI Sprite References")]
    [Tooltip("The sprite for the button prompt on controller")]
    [SerializeField] private Sprite controllerButtonSprite;
    [Tooltip("The sprite for the button prompt on keyboard")]
    [SerializeField] private Sprite keyboardButtonSprite;
    private GameObject buttonMashUI;
    private Image buttonImage;
    private TextMeshProUGUI mashText;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private Quaternion lyingLocalRotation;
    private Vector3 lyingLocalPosition;
    private Quaternion uprightLocalRotation;
    private Vector3 uprightLocalPosition;
    private float progress; // 0 == lying, 1 == upright
    private bool minigameActive;
    private Rigidbody rb;
    private bool complete = false;
    private Coroutine pulseRoutine;

    [HideInInspector] public bool isInteractable = true;
    public float interactionPriority => 1;
    public InteractType interactType => InteractType.Move; // Should be InteractType.Push once the icon for that is made
    public event Action OnSuccess;

    // cached player state while minigame runs
    private PlayerController frozenPlayerController;
    private Rigidbody frozenPlayerRb;

    private void Awake()
    {
        // store the initial rotation and position as the lying (start) state
        lyingLocalRotation = transform.localRotation;
        lyingLocalPosition = transform.localPosition;

        if (referenceTransform == null)
        {
            Debug.LogError("Reference Transform not assigned on PushableObject. Please assign a reference transform in the inspector.", this);
            enabled = false;
            return;
        }

        // set upright local position from reference
        uprightLocalPosition = referenceTransform.localPosition;

        // compute upright rotation using a plausible tilt axis derived from reference position
        ComputeUprightRotationFromReference();

        referenceTransform.gameObject.SetActive(!disableReferenceObject);

        rb = GetComponent<Rigidbody>();

        if (GameManager.Instance != null && GameManager.Instance.qteButtonMashUI != null)
        {
            buttonMashUI = GameManager.Instance.qteButtonMashUI;
            buttonImage = buttonMashUI.GetComponentInChildren<Image>();
            mashText = buttonMashUI.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonImage != null && controllerButtonSprite != null && keyboardButtonSprite != null)
            {
                // Set the appropriate sprite based on the current input device
                SetButtonSprite();
            }
            else
            {
                Debug.LogError("Button image or sprites not assigned for PushableObject UI. Check GameManager references");
            }

            if (mashText != null)
            {
                mashText.text = "Mash!";
            }
            else
            {
                Debug.LogError("Mash text object not assigned for PushableObject UI. Check GameManager references");
            }

            buttonMashUI.SetActive(false);
        }

        progress = 0f;
        minigameActive = false;
    }

    private void ComputeUprightRotationFromReference()
    {
        // world direction from object to reference
        Vector3 worldDir = referenceTransform.position - transform.position;

        // project to world horizontal plane (use world up so result is "behind/in front" relative to world)
        Vector3 dirProj = Vector3.ProjectOnPlane(worldDir, Vector3.up);

        Vector3 axisWorld;
        if (dirProj.sqrMagnitude > Mathf.Epsilon)
        {
            // axis that will produce a "pitch" lifting the object toward the reference direction:
            // cross(up, dirProj) gives an axis that when rotated will pitch the object toward the reference
            axisWorld = Vector3.Cross(Vector3.up, dirProj.normalized);
            if (axisWorld.sqrMagnitude < Mathf.Epsilon)
            {
                // degenerate, fall back
                axisWorld = transform.right;
            }
        }
        else
        {
            // reference is almost directly above/below or extremely close, default to object's local right axis
            axisWorld = transform.right;
        }

        // convert axis to local space and normalize
        Vector3 axisLocal = transform.InverseTransformDirection(axisWorld).normalized;
        if (axisLocal.sqrMagnitude < Mathf.Epsilon)
        {
            axisLocal = Vector3.right;
        }

        // Build upright rotation relative to the lying local rotation
        uprightLocalRotation = lyingLocalRotation * Quaternion.AngleAxis(rotationAngle, axisLocal);

        uprightLocalPosition = referenceTransform.localPosition;
    }

    private void SetButtonSprite()
    {
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.InputMode inputMode = InputDeviceManager.Instance.CurrentMode;

            if (inputMode == InputDeviceManager.InputMode.Controller && controllerButtonSprite != null)
            {
                buttonImage.sprite = controllerButtonSprite;
            }
            else if (inputMode == InputDeviceManager.InputMode.KeyboardMouse && keyboardButtonSprite != null)
            {
                buttonImage.sprite = keyboardButtonSprite;
            }
        }
    }

    void OnEnable()
    {
        if (mashInput != null && mashInput.action != null)
        {
            mashInput.action.performed += ReadMash;
            mashInput.action.canceled += ReadMash;
        }

        if (quitInput != null)
        {
            quitInput.action.performed += ReadQuit;
            quitInput.action.canceled += ReadQuit;
        }
    }

    void OnDisable()
    {
        if (mashInput != null)
        {
            mashInput.action.performed -= ReadMash;
            mashInput.action.canceled -= ReadMash;
        }

        if (quitInput != null)
        {
            quitInput.action.performed -= ReadQuit;
            quitInput.action.canceled -= ReadQuit;
        }
    }

    private void ReadMash(InputAction.CallbackContext context)
    {
        if (minigameActive && context.action.triggered)
        {
            SetButtonSprite(); // update sprite in case input device changed since minigame started
            progress += mashProgressPerPress;
            // pulse the UI button
            if (buttonImage != null)
            {
                StartPulse();
            }
        }
    }

    private void ReadQuit(InputAction.CallbackContext context)
    {
        if (minigameActive && context.action.triggered)
        {
            EndMinigame(false);
        }
    }

    private void Update()
    {
        if (!minigameActive) return;

        // passive decrease
        progress -= passiveDecay * Time.deltaTime;
        progress = Mathf.Clamp01(progress);

        // apply rotation and position interpolation (local space)
        transform.SetLocalPositionAndRotation(
            Vector3.Lerp(lyingLocalPosition, uprightLocalPosition, progress),
            Quaternion.Slerp(lyingLocalRotation, uprightLocalRotation, progress)
        );

        // check for completion
        if (progress >= 1f)
        {
            EndMinigame(true);
        }
    }

    public bool CanInteract(GameObject player)
    {
        // allow interaction only when minigame isn't active / not complete
        return isInteractable && !minigameActive && !complete;
    }

    public void OnPlayerInteraction(GameObject player)
    {
        if (minigameActive) return;

        // start minigame
        if (showDebugLogs) Debug.Log("Started push minigame", this);
        minigameActive = true;
        progress = 0f;

        // recompute lying/upright local states in case object/reference moved at runtime
        lyingLocalRotation = transform.localRotation;
        lyingLocalPosition = transform.localPosition;
        ComputeUprightRotationFromReference();

        // show UI
        if (buttonMashUI != null)
        {
            buttonMashUI.SetActive(true);
        }
        if (buttonImage != null)
        {
            SetButtonSprite();
            buttonImage.gameObject.SetActive(true);
            // ensure scale reset
            buttonImage.rectTransform.localScale = Vector3.one;
        }
        if (mashText != null)
        {
            mashText.gameObject.SetActive(true);
        }

        if (freezePlayerDuringMinigame && player != null)
        {
            frozenPlayerController = player.GetComponent<PlayerController>();
            frozenPlayerRb = player.GetComponent<Rigidbody>();

            if (frozenPlayerController != null)
            {
                frozenPlayerController.MovementLocked = true;
                frozenPlayerController.enabled = false;
            }

            if (frozenPlayerRb != null)
            {
                // freeze the player rigidbody movement during minigame
                frozenPlayerRb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        // Ensure object starts from its lying local transform
        transform.SetLocalPositionAndRotation(lyingLocalPosition, lyingLocalRotation);
    }

    private void EndMinigame(bool succeeded)
    {
        minigameActive = false;

        // hide UI
        if (buttonMashUI != null)
        {
            buttonMashUI.SetActive(false);
        }
        if (buttonImage != null)
            buttonImage.gameObject.SetActive(false);
        if (mashText != null)
            mashText.gameObject.SetActive(false);

        // stop any pulse coroutine
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
            if (buttonImage != null)
                buttonImage.rectTransform.localScale = Vector3.one;
        }

        // restore player
        if (freezePlayerDuringMinigame && frozenPlayerController != null)
        {
            frozenPlayerController.MovementLocked = false;
            frozenPlayerController.enabled = true;
        }

        if (freezePlayerDuringMinigame && frozenPlayerRb != null)
        {
            frozenPlayerRb.constraints = RigidbodyConstraints.None;
            frozenPlayerRb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (showDebugLogs) Debug.Log(succeeded ? "Push minigame succeeded" : "Push minigame failed", this);

        if (succeeded)
        {
            // ensure final upright local transform applied
            transform.SetLocalPositionAndRotation(uprightLocalPosition, uprightLocalRotation);
            rb.constraints = RigidbodyConstraints.FreezeAll;
            OnSuccess?.Invoke();
            // disable future interactions
            complete = true;
            enabled = false;
        }
        else
        {
            // Reset the object back to lying
            progress = 0f;
            transform.SetLocalPositionAndRotation(lyingLocalPosition, lyingLocalRotation);
        }
    }

    private void StartPulse()
    {
        if (buttonImage == null) return;
        // restart pulse
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseButtonRoutine());
    }

    private System.Collections.IEnumerator PulseButtonRoutine()
    {
        RectTransform rt = buttonImage.rectTransform;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * Mathf.Max(0.01f, pulseScale);

        float half = pulseDuration * 0.5f;
        float t = 0f;

        // scale up
        while (t < half)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / half);
            rt.localScale = Vector3.Lerp(startScale, targetScale, f);
            yield return null;
        }

        // scale down
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / half);
            rt.localScale = Vector3.Lerp(targetScale, startScale, f);
            yield return null;
        }

        rt.localScale = startScale;
        pulseRoutine = null;
    }
}
