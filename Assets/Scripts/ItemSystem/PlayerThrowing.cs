using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerThrowing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private PlayerEquipItem playerEquipItem;
    [SerializeField] private GameObject interactingScript;
    private InventoryUIController inventoryUI;
    private ToggleInventoryUI inventoryToggle;
    
    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 1f;
    [SerializeField] private float maxThrowForce = 10f;
    [SerializeField] private float upwardForce = 2f;
    [SerializeField] private float chargeSpeed = 1.5f;

    [Header("Input")]
    [SerializeField] private MouseButton chargeKey = MouseButton.Left;
    [SerializeField] private string chargeButton = "XboxRightTrigger";

    private int chargeKeyInt;
    private bool isCharging = false;
    private float currentCharge = 0f;
    private bool usingController;

    private void Start()
    {
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
            powerSlider.gameObject.SetActive(false);
        }

        chargeKeyInt = (int)chargeKey;
    }
    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerEquipItem == null)
        {
            playerEquipItem = GetComponent<PlayerEquipItem>();
        }

        if (interactingScript != null && inventoryUI == null)
        {
            inventoryUI = interactingScript.GetComponent<InventoryUIController>();
        }

        if (inventoryToggle == null)
        {
            inventoryToggle = GetComponent<ToggleInventoryUI>();
        }
    }

    // Update is called once per frame
    private void Update()
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
        }

        if ((Input.GetAxis(chargeButton) > 0.1f) && !isCharging)
        {
            isCharging = true;
            currentCharge = 0f;
            usingController = true;
        }

        if (isCharging)
        {
            currentCharge += chargeSpeed * Time.deltaTime;

            float normalized = Mathf.PingPong(currentCharge, 1f);

            if (powerSlider != null)
            {
                powerSlider.value = normalized;
                powerSlider.gameObject.SetActive(true);
            }

            if (usingController)
            {
                if (Input.GetAxis(chargeButton) < 0.1f)
                {
                    ThrowItem(normalized, true);
                    isCharging = false;

                    if (powerSlider != null)
                    {
                        powerSlider.value = 0f;
                        powerSlider.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(chargeKeyInt))
                {
                    ThrowItem(normalized, false);
                    isCharging = false;

                    if (powerSlider != null)
                    {
                        powerSlider.value = 0f;
                        powerSlider.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void ThrowItem(float normalizedPower, bool usingController)
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

            if (usingController)
            {
                Vector3 aimDirection = CalculateInputFromPOV();

                if (aimDirection.magnitude < 0.01f)
                {
                    Debug.Log("Stick at neutral");
                    aimDirection = transform.forward;
                }

                direction = aimDirection.normalized;
            }
            else
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 targetPoint;

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    targetPoint = hit.point;   
                }
                else
                {
                    targetPoint = ray.GetPoint(30f);   
                }

                direction = (targetPoint - transform.position).normalized;
            }

            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, normalizedPower);
            Debug.Log("Direction: " + direction + ", ThrowForce: " + throwForce + ", NormalizedPower: " + normalizedPower);
            rb.AddForce(direction * throwForce + Vector3.up * upwardForce, ForceMode.Impulse);
        }
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

        Vector3 relativeDirection = (camForward * input.x + camRight * input.z).normalized;
        return relativeDirection;
    }
}
