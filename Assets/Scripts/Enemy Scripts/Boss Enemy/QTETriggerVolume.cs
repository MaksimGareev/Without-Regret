using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTETriggerVolume : MonoBehaviour, IInteractable
{
    [Header("QTE Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField, Tooltip("The number of inputs required to complete the QTE")] private int numInputs = 4;    

    [Header("Arrow UI")]
    [SerializeField, Tooltip("The QTE Canvas object")] private GameObject qteCanvas;
    [SerializeField, Tooltip("Prefab for a single arrow RawImage (used to duplicate arrows at runtime).")] private RawImage arrowPrefab;
    [SerializeField, Tooltip("Container RectTransform which will hold generated arrows.")] private RectTransform arrowsContainer;
    [SerializeField, Tooltip("The sprite assets for each arrow. Should have 4 (in order of up->right->down->left)")] private Sprite[] arrowImages;
    [SerializeField, Tooltip("Horizontal spacing (in pixels) between generated arrows")] private float arrowSpacing = 8f;

    [Header("References")]
    [SerializeField] private BossEnemyController bossEnemy;
    [SerializeField] private PlayerController playerController;
    [SerializeField, Tooltip("The platforms that this qte controls.")] private List<OrbitingPlatform> platforms;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = false;

    // runtime state
    private int[] directionAssignments; // 0 = up, 1 = left, 2 = down, 3 = right
    private int arrowIndex = 0;
    private int arrowsLength;
    private PlayerControls controls;
    private bool controlsLocked = false;
    private bool initiated = false;

    private readonly List<RawImage> runtimeArrows = new List<RawImage>();

    public float interactionPriority => 10;

    public InteractType interactType => InteractType.BossQTE;

    private void Awake()
    {
        controls = new PlayerControls();
        qteCanvas.SetActive(false);
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        if (!initiated) return;

        // ensure we have assignments
        if (directionAssignments == null || directionAssignments.Length == 0) return;

        if (!controlsLocked && arrowIndex < directionAssignments.Length)
        {
            // Check the player's input
            if (controls.LockPicking.ArrowUp.triggered)
            {
                CheckDirection(0);
            }
            else if (controls.LockPicking.ArrowRight.triggered)
            {
                CheckDirection(1);
            }
            else if (controls.LockPicking.ArrowDown.triggered)
            {
                CheckDirection(2);
            }
            else if (controls.LockPicking.ArrowLeft.triggered)
            {
                CheckDirection(3);
            }
        }
        else if (arrowIndex >= directionAssignments.Length)
        {
            // Succeeded
            EndQTESuccess();
        }
    }

    public bool CanInteract(GameObject player)
    {
        return !initiated;
    }

    public void OnPlayerInteraction(GameObject player)
    {
        // Prepare UI and sequence
        SetupArrowUI();

        // Generate sequence for the QTE, freeze player movement, and initialize UI
        GenerateSolutions();

        playerController.MovementLocked = true;
        playerController.enabled = false;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        // Reset state
        arrowIndex = 0;
        controlsLocked = false;
        initiated = true;

        qteCanvas.SetActive(true);
        if (showDebugLogs) Debug.Log("Starting QTE");
    }

    private void SetupArrowUI()
    {
        // clean any previous runtime arrows
        runtimeArrows.Clear();

        if (arrowPrefab == null || arrowsContainer == null)
        {
            Debug.LogError("QTETriggerVolume: arrowPrefab or arrowsContainer not assigned. Cannot create QTE UI.");
            return;
        }

        // remove previous children (safe clear)
        for (int i = arrowsContainer.childCount - 1; i >= 0; --i)
            Destroy(arrowsContainer.GetChild(i).gameObject);

        // ensure container width is available (default to prefab width if zero)
        float containerWidth = Mathf.Max(0.0001f, arrowsContainer.rect.width);
        float spacing = Mathf.Max(0f, arrowSpacing);
        float totalSpacing = spacing * (numInputs - 1);
        // Arrows should be sized to fit within the container with the specified spacing, but not be stretched beyond their original width
        float widthPer = Mathf.Min(arrowPrefab.GetComponent<RectTransform>().rect.width, (containerWidth - totalSpacing) / numInputs);

        for (int i = 0; i < numInputs; i++)
        {
            RawImage arrow = Instantiate(arrowPrefab, arrowsContainer);
            arrow.color = Color.white;
            arrow.gameObject.SetActive(true);
            runtimeArrows.Add(arrow);

            // position & size the arrow within the container, accounting for spacing
            if (arrow.TryGetComponent<RectTransform>(out var rect))
            {
                // Use left-centered anchors so anchoredPosition.x is from left edge of container
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);

                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthPer);

                float xPos = (widthPer * 0.5f) + i * (widthPer + spacing);
                rect.anchoredPosition = new Vector2(xPos, 0f);
            }
        }

        arrowsLength = runtimeArrows.Count;
        directionAssignments = new int[arrowsLength];
    }

    private void GenerateSolutions()
    {
        if (arrowImages == null || arrowImages.Length < 4)
        {
            Debug.LogError("QTETriggerVolume: Not enough arrow images assigned. Please assign 4 arrow sprites in the inspector.");
            return;
        }

        for (int i = 0; i < arrowsLength; i++)
        {
            int direction = Random.Range(0, arrowImages.Length);
            directionAssignments[i] = direction;

            // assigns sprites to the ui images based off directional inputs given
            if (runtimeArrows[i] != null)
            {
                runtimeArrows[i].texture = arrowImages[direction].texture;
            }
        }
    }

    private void CheckDirection(int input)
    {
        if (input == directionAssignments[arrowIndex])
        {
            // Correct direction, move on
            if (runtimeArrows[arrowIndex] != null)
                runtimeArrows[arrowIndex].color = new Color(0f, 1f, 0f, 0.75f); // Green with slight transparency
            arrowIndex++;
        }
        else
        {
            arrowIndex = 0;
            controlsLocked = true;
            // Wrong Direction, start over
            StartCoroutine(WrongDirection());
        }
    }

    IEnumerator WrongDirection()
    {
        if (showDebugLogs) Debug.Log("Wrong Direction input, resetting");
        for (int i = 0; i < runtimeArrows.Count; i++)
        {
            if (runtimeArrows[i] != null)
                runtimeArrows[i].color = Color.red;
        }

        yield return new WaitForSeconds(.25f);

        for (int i = 0; i < runtimeArrows.Count; i++)
        {
            if (runtimeArrows[i] != null)
                runtimeArrows[i].color = Color.white;
        }
        controlsLocked = false;
    }

    private void EndQTESuccess()
    {
        qteCanvas.SetActive(false);

        // Unlock player movement
        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        playerController.MovementLocked = false;
        playerController.enabled = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        initiated = false;

        if (showDebugLogs) Debug.Log("QTE succeeded");

        // Deal damage to boss enemy and disable trigger volume
        if (bossEnemy != null)
        {
            bossEnemy.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("Boss Enemy reference for QTE trigger volume is null!", this);
        }

        // Set platforms to QTE complete and increase their speed
        for (int i = 0; i < platforms.Count; i++)
        {
            platforms[i].SetQTEComplete();
            platforms[i].orbitSpeed *= 2;
        }

        // cleanup runtime arrows
        if (arrowPrefab != null && arrowsContainer != null)
        {
            for (int i = arrowsContainer.childCount - 1; i >= 0; --i)
                Destroy(arrowsContainer.GetChild(i).gameObject);
        }

        // Save game after successful QTE
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        gameObject.SetActive(false);
    }

    public void OnDrawGizmos()
    {
        // Visualize the trigger volume
        Gizmos.color = Color.purple;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
