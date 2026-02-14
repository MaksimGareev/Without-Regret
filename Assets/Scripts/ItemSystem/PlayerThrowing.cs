using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerThrowing : MonoBehaviour
{
    
    private Inventory inventory;
    private Transform throwOrigin;
    private Camera playerCamera;
    private PlayerEquipItem playerEquipItem;
    private GameObject interactingScript;
    private GameObject WorldThrowPointer;

    [Header("Throwing Pointer and UI Settings")]
    [SerializeField] private float MaxPointerLength = 10;
    [SerializeField] private float MinPointerLength = 1;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private LineRenderer line;
    [SerializeField] [Range(10, 100)] private int linePoints = 25;
    [SerializeField] [Range(0.01f, 0.25f)] private float timeBetweenPoints = 0.1f;
    [SerializeField] private LayerMask lineLayerMask;//set layers here that the throwables collide with.
    private InventoryUIController inventoryUI;
    private ToggleInventoryUI inventoryToggle;
    
    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 1f;
    [SerializeField] private float maxThrowForce = 10f;
    [SerializeField] private float upwardForceMax = 10f;
    [SerializeField] private float upwardForceMin = 2f;
    private float currentUpwardForce;
    [SerializeField] private float chargeSpeed = .5f;
    [SerializeField] private float holdTime = 5f;
    private float currentHoldTime = 0f;

    [Header("Input")]
    [SerializeField] private MouseButton chargeKey = MouseButton.Right;
    [SerializeField] private string chargeButton = "Xbox RightStick Click";

    private int chargeKeyInt;
    private bool isCharging = false;
    private float currentCharge = 0f;
    private bool usingController;
    private Vector3 PointerScale;

    private Animator animator;

    private void Start()
    {
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
            powerSlider.gameObject.SetActive(false);
        }
        WorldThrowPointer.SetActive(false);
        PointerScale = WorldThrowPointer.transform.localScale;
        line.enabled = false;
        chargeKeyInt = (int)chargeKey;
    }
    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on the player or its children. Please ensure an Animator component is added to the player's 'Echo' mesh.");
            }
        }

        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
            if (inventory == null)
            {
                Debug.LogError("Inventory component not found on the player. Please ensure an Inventory component is added to the player.");
            }
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("Main Camera not found. Please ensure there is a Camera in the scene tagged as 'MainCamera'.");
            }
        }

        if (playerEquipItem == null)
        {
            playerEquipItem = GetComponent<PlayerEquipItem>();
            if (playerEquipItem == null)
            {
                Debug.LogError("PlayerEquipItem component not found on the player. Please ensure a PlayerEquipItem component is added to the player.");
            }
        }

        if (throwOrigin == null)
        {
            throwOrigin = GameObject.Find("Throwing Origin")?.transform;
            if (throwOrigin == null)
            {
                Debug.LogError("Throwing Origin GameObject with Transform component not found in the scene. Please ensure a GameObject named 'Throwing Origin' exists as a child of the player.");
            }
        }
        
        if (powerSlider == null)
        {
            powerSlider = GameObject.Find("ThrowingSlider")?.GetComponent<Slider>();
            if (powerSlider == null)
            {
                Debug.LogError("ThrowingSlider GameObject with Slider component not found in the scene. Please ensure a GameObject named 'ThrowingSlider' exists in the MainCanvas prefab with a Slider component attached.");
            }
        }

        if (WorldThrowPointer == null)
        {
            WorldThrowPointer = GameObject.Find("WorldThrowIndicator");
            if (WorldThrowPointer == null)
            {
                Debug.LogError("WorldThrowIndicator GameObject not found in the scene. Please ensure a GameObject named 'WorldThrowIndicator' exists in the scene as a child of the player.");
            }
        }

        if (interactingScript == null)
        {
            var foundObjects = FindObjectsByType<InventoryUIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            interactingScript = foundObjects.Length > 0 ? foundObjects[0].gameObject : null;
            if (interactingScript == null)
            {
                Debug.LogError("InteractingScript GameObject not found in the scene. Please ensure a GameObject named 'InteractingScript' exists in the scene as a child of the Inventory UI in the MainCanvas.");
            }
        }

        if (interactingScript != null && inventoryUI == null)
        {
            inventoryUI = interactingScript.GetComponent<InventoryUIController>();
            if (inventoryUI == null)
            {
                Debug.LogError("InventoryUIController component not found on the interactingScript GameObject. Please ensure an InventoryUIController component is added to the interactingScript GameObject.");
            }
        }

        if (inventoryToggle == null)
        {
            inventoryToggle = GetComponent<ToggleInventoryUI>();
            if (inventoryToggle == null)
            {
                Debug.LogError("ToggleInventoryUI component not found on the player. Please ensure a ToggleInventoryUI component is added to the player.");
            }
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (Time.timeScale != 0f && playerEquipItem.throwableEquipped && !inventoryToggle.isEnabled)
        {
            HandleCharging();
        }
        else if (Time.timeScale != 0f && playerEquipItem.grabbableEquipped && (Input.GetMouseButtonDown(chargeKeyInt) || Input.GetAxis(chargeButton) > 0.1f))
        {
            DropItem();
        }
    }

    private void DropItem()
    {
        ItemData itemToDrop = playerEquipItem.currentEquippedItem;
        if (itemToDrop == null)
        {
            return;
        }

        inventoryUI.RefreshInventoryUI();
        playerEquipItem.UnequipItem();
        Instantiate(itemToDrop.WorldPrefab, transform.position + transform.forward * 1f, Quaternion.identity);
    }

    private void HandleCharging()
    {
        if (Input.GetMouseButtonDown(chargeKeyInt) && !isCharging)
        {
            isCharging = true;
            currentCharge = 0f;
            usingController = false;
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.Confined;
        }

        if ((Input.GetAxis(chargeButton) > 0.1f) && !isCharging)
        {
            isCharging = true;
            currentCharge = 0f;
            usingController = true;
        }

        if (isCharging)
        {
            if (currentCharge < 1f)
            {
                currentCharge += chargeSpeed * Time.deltaTime;

                //float normalized = Mathf.PingPong(currentCharge, 1f);
            }
            else if (currentCharge >= 1f)
            {
                if(currentHoldTime < holdTime)
                {
                    currentHoldTime += Time.deltaTime;
                }
                else
                {
                    ThrowItem(currentCharge);
                    //animator.SetTrigger("Throw"); //play throw animation when button is released
                    return;
                }
            }
            DrawProjection();

            if (powerSlider != null)
            {
                powerSlider.value = currentCharge;
                powerSlider.gameObject.SetActive(true);
            }

            if (WorldThrowPointer != null)
            {
                //    PointerScale = Vector3.Lerp(new Vector3(MinPointerLength, PointerScale.y, PointerScale.z), new Vector3(MaxPointerLength, PointerScale.y, PointerScale.z), currentCharge);
                //    WorldThrowPointer.transform.localScale = PointerScale;
                WorldThrowPointer.gameObject.SetActive(true);
            }

            if (usingController)
            {
                if (Input.GetAxis(chargeButton) < 0.1f)
                {
                    ThrowItem(currentCharge);
                    //animator.SetTrigger("Throw"); //play throw animation when button is released
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(chargeKeyInt))
                {
                    ThrowItem(currentCharge);
                    //animator.SetTrigger("Throw"); //play throw animation when button is released
                }
            }
            
        }
        else
        {
            line.enabled = false;
        }
        

    }

    private void ThrowItem(float charge)
    {
        ItemData itemToThrow = inventory.GetFirstThrowable();
        if (itemToThrow == null)
        {
            return;
        }

        inventory.RemoveItem(itemToThrow);
        inventoryUI.RefreshInventoryUI();
        playerEquipItem.UnequipItem();

        GameObject gameObject = Instantiate(itemToThrow.WorldPrefab, throwOrigin.position, Quaternion.identity);

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction;

                Vector3 aimDirection = CalculateInputFromPOV();

                if (aimDirection.magnitude < 0.01f)
                {
                    Debug.Log("Stick at neutral");
                    aimDirection = transform.forward;
                }

                direction = aimDirection.normalized;

            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, charge);
            currentUpwardForce = Mathf.Lerp(upwardForceMax, upwardForceMin, charge);
            rb.AddForce(direction * throwForce + Vector3.up * currentUpwardForce, ForceMode.Impulse);
        }
        if (powerSlider != null)
        {
            powerSlider.value = 0f;
            powerSlider.gameObject.SetActive(false);
            PointerScale.x = MinPointerLength;
            WorldThrowPointer.transform.localScale = PointerScale;
            WorldThrowPointer.SetActive(false);
            line.enabled = false;
        }
        currentHoldTime = 0;
        isCharging = false;
    }

    private Vector3 CalculateInputFromPOV()
    {
        Vector3 input = new Vector3(Input.GetAxis("Xbox RightStick X"), 0, Input.GetAxis("Xbox RightStick Y"));

        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 relativeDirection = (camRight * input.x + camForward * input.z).normalized;
        return relativeDirection;
    }

    private void DrawProjection()
    {
        line.enabled = true;
        line.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        Vector3 startPosition = throwOrigin.position;
        float currentThrowForce = Mathf.Lerp(minThrowForce, maxThrowForce, currentCharge);
        currentUpwardForce = Mathf.Lerp(upwardForceMax, upwardForceMin, currentCharge);
        Vector3 direction;

        Vector3 aimDirection = CalculateInputFromPOV();

        if (aimDirection.magnitude < 0.01f)
        {
            Debug.Log("Stick at neutral");
            aimDirection = transform.forward;
        }
        direction = aimDirection.normalized;
        Vector3 startVelocity = direction * currentThrowForce + Vector3.up * currentUpwardForce / 1;
        int i = 0;
        line.SetPosition(i, startPosition);
        for (float time = 0; time < linePoints; time += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            line.SetPosition(i, point);

            Vector3 lastPosition = line.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, lineLayerMask))
            {
                line.SetPosition(i, hit.point);
                WorldThrowPointer.transform.position = hit.point;
                line.positionCount = i + 1;
                return;
            }
        }

    }

    public bool GetIsCharging() => isCharging;
}
