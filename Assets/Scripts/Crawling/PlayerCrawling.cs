using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrawling : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference crawlAction;

    private PlayerControls controls;
    private Vector2 moveInput = Vector2.zero;
    private bool crawlInput = false;

    [Header("Crawl Settings")]
    [SerializeField] private float crawlSpeed = 1f;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Collider regularCollider;
    [SerializeField] private Collider crawlingCollider;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private bool isCrawling = false;
    private PlayerController playerController;
    private PlayerFloating playerFloating;
    private PlayerMovingObjects playerMovingObjects;
    private PlayerPossessing playerPossessing;
    private PlayerThrowing playerThrowing;
    private Camera playerCamera;
    private Rigidbody rb;
    private CharacterController controller;
    private ToggleInventoryUI inventoryToggle;

    private bool prevRbUseGravity;
    private bool prevRbKinematic;


    void OnEnable()
    {
        controls.Enable();
        crawlAction.action.Enable();
    }
    void OnDisable()
    {
        controls.Disable();
        crawlAction.action.Disable();
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerFloating = GetComponent<PlayerFloating>();
        playerMovingObjects = GetComponent<PlayerMovingObjects>();
        playerPossessing = GetComponent<PlayerPossessing>();
        playerThrowing = GetComponent<PlayerThrowing>();
        playerCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        inventoryToggle = GetComponent<ToggleInventoryUI>();

        controls = new PlayerControls();
        crawlAction.action.performed += ctx => ReadCrawl(ctx);
        crawlAction.action.canceled += ctx => ReadCrawl(ctx);
        controls.Player.Move.performed += ctx => ReadMove(ctx);
        controls.Player.Move.canceled += ctx => ReadMove(ctx);
    }

    // Update is called once per frame
    void Update()
    {
        if (crawlInput && !inventoryToggle.isEnabled)
        {
            crawlInput = false;
            isCrawling = !isCrawling;
            if (isCrawling)
            {
                StartCrawling();
            }
            else
            {
                StopCrawling();
            }
        }

        if (isCrawling)
        {
            ApplyCrawlingMovement();
        }
    }

    public void ReadMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void ReadCrawl(InputAction.CallbackContext context)
    {
        crawlInput = context.action.triggered;

        if (showDebugLogs)
        {
            Debug.Log("PlayerCrawling - Crawl Input: " + crawlInput);
        }
    }

    private void StartCrawling()
    {
        if (showDebugLogs)
        {
            Debug.Log("Player started crawling");
        }

        prevRbUseGravity = rb.useGravity;
        prevRbKinematic = rb.isKinematic;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerFloating != null)
        {
            playerFloating.enabled = false;
        }

        if (playerMovingObjects != null)
        {
            playerMovingObjects.enabled = false;
        }

        if (playerPossessing != null)
        {
            playerPossessing.enabled = false;
        }

        if (playerThrowing != null)
        {
            playerThrowing.enabled = false;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        // switch collider (rotation is temp until animations)
        ApplyColliderForCrawl(true);
        playerModel.transform.Rotate(Vector3.right * 90f);
    }

    private void StopCrawling()
    {
        if (showDebugLogs)
        {
            Debug.Log("Player stopped crawling");
        }

        rb.isKinematic = prevRbKinematic;
        rb.useGravity = prevRbUseGravity;

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerFloating != null)
        {
            playerFloating.enabled = true;
        }

        if (playerMovingObjects != null)
        {
            playerMovingObjects.enabled = true;
        }

        if (playerPossessing != null)
        {
            playerPossessing.enabled = true;
        }

        if (playerThrowing != null)
        {
            playerThrowing.enabled = true;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        // switch collider (rotation is temp until animations)
        ApplyColliderForCrawl(false);
        playerModel.transform.Rotate(Vector3.right * -90f);
    }

    private void ApplyCrawlingMovement()
    {
        Vector3 move = GetRawCameraRelativeInput();

        transform.Translate(move * crawlSpeed * Time.deltaTime, Space.World);

        // Rotate the player to face the way they are moving
        if (move.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void ApplyColliderForCrawl(bool crawling)
    {
        regularCollider.enabled = !crawling;
        crawlingCollider.enabled = crawling;
    }

    private Vector3 GetRawCameraRelativeInput()
    {
        // Get input values
        float x = moveInput.x;
        float z = moveInput.y;
        Vector3 input = new Vector3(x, 0f, z);

        // early out: no input
        if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

        // Get camera rotation vectors
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0f; camRight.Normalize();

        // combine 
        Vector3 world = camForward * input.z + camRight * input.x;
        return world;
    }
}