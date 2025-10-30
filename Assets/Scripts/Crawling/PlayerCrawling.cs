using UnityEngine;

public class PlayerCrawling : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode crawlKey = KeyCode.C;
    [SerializeField] private string crawlButton = "Xbox B Button";

    [Header("Crawl Settings")]
    [SerializeField] private float crawlSpeed = 1f;
    [SerializeField] private GameObject playerModel;

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
    }
    
    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown(crawlKey) || Input.GetButtonDown(crawlButton)) && !inventoryToggle.isEnabled)
        {
            isCrawling = !isCrawling;
            switch (isCrawling)
            {
                case true:
                    StartCrawling();
                    break;
                case false:
                    StopCrawling();
                    break;
            }
        }

        if (isCrawling)
        {
            ApplyCrawlingMovement();
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

        // Temporary until crawling animation is created
        playerModel.transform.Rotate(Vector3.right * 90);
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

        playerModel.transform.Rotate(-Vector3.right * 90);
    }
    
    private Vector3 GetRawCameraRelativeInput()
    {
        // Get input values
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(x, 0f, z);

        // early out: no input
        if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

        // Get camera rotation vectors
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        
        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0f; 
        camRight.Normalize();

        // combine 
        Vector3 world = camForward * input.z + camRight * input.x;
        return world;
    }
}
