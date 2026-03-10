using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerThrowing : MonoBehaviour
{
    
    private Inventory inventory;
    private Transform throwOrigin;
    private Camera playerCamera;
    private PlayerEquipItem playerEquipItem;
    private GameObject WorldThrowPointer;

    [Header("Throwing Pointer and UI Settings")]
    //[SerializeField] private Slider powerSlider;
    [SerializeField] private LineRenderer line;
    [SerializeField] [Range(10, 100)] private int linePoints = 25;
    [SerializeField] [Range(0.01f, 0.25f)] private float timeBetweenPoints = 0.1f;
    [SerializeField] private LayerMask lineLayerMask;//set layers here that the throwables collide with.
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

    public Animator animator;

    private void Start()
    {
        if (GameManager.Instance.throwingSlider != null)
        {
            GameManager.Instance.throwingSlider.minValue = 0f;
            GameManager.Instance.throwingSlider.maxValue = 1f;
            GameManager.Instance.throwingSlider.value = 0f;
            GameManager.Instance.throwingSlider.gameObject.SetActive(false);
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
            //animator = GetComponentInChildren<Animator>();
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

        if (WorldThrowPointer == null)
        {
            WorldThrowPointer = GameObject.Find("WorldThrowIndicator");
            if (WorldThrowPointer == null)
            {
                Debug.LogError("WorldThrowIndicator GameObject not found in the scene. Please ensure a GameObject named 'WorldThrowIndicator' exists in the scene as a child of the player.");
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

        GameManager.Instance.inventoryInteractingScript.RefreshInventoryUI();
        playerEquipItem.UnequipItem();
        Instantiate(itemToDrop.WorldPrefab, transform.position + transform.forward * 1f, Quaternion.identity);
    }

    private void HandleCharging()
    {
        if (Input.GetMouseButtonDown(chargeKeyInt) && !isCharging)
        {
            StartCharging(false);
        }

        if ((Input.GetAxis(chargeButton) > 0.1f) && !isCharging)
        {
            StartCharging(true);
        }

        if (isCharging)
        {
            if (currentCharge < 1f) // increases charge amount with time
            {
                currentCharge += chargeSpeed * Time.deltaTime;
            }
            else if (currentCharge >= 1f) //when at max charge, starts a timer for how long the player can hold that max charge
            {
                if(currentHoldTime < holdTime)
                {
                    currentHoldTime += Time.deltaTime;
                }
                else // when timer runs out, automatically throws the item and stops further functionality
                {
                    ThrowItem(currentCharge);
                    return;
                }
            }
            DrawProjection();

            if (GameManager.Instance.throwingSlider != null)
            {
                GameManager.Instance.throwingSlider.value = currentCharge;
                GameManager.Instance.throwingSlider.gameObject.SetActive(true);
            }

            if (WorldThrowPointer != null)
            {
                WorldThrowPointer.gameObject.SetActive(true);
            }

            if (usingController)
            {
                if (Input.GetAxis(chargeButton) < 0.1f)
                {
                    ThrowItem(currentCharge);
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(chargeKeyInt))
                {
                    ThrowItem(currentCharge);
                }
            }
            
        }
        else
        {
            line.enabled = false;
        }
        

    }

    private void ThrowItem(float charge)// throws the item
    {
        ItemData itemToThrow = inventory.GetFirstThrowable();
        if (itemToThrow == null)
        {
            return;
        }

        inventory.RemoveItem(itemToThrow);
        GameManager.Instance.inventoryInteractingScript.RefreshInventoryUI();
        playerEquipItem.UnequipItem();

        StartCoroutine(ThrowAnimHandler());// plays throw animation

        GameObject gameObject = Instantiate(itemToThrow.WorldPrefab, throwOrigin.position, Quaternion.identity);

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction;

            Vector3 aimDirection = transform.forward;

            direction = aimDirection.normalized;

            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, charge);
            currentUpwardForce = Mathf.Lerp(upwardForceMax, upwardForceMin, charge);// upward force dependant on how long charge is held. its at max when charging just begins.
            Vector3 finalForce = direction * throwForce + Vector3.up.normalized * currentUpwardForce;
            rb.AddForce(finalForce, ForceMode.Impulse);
        }
        if (GameManager.Instance.throwingSlider != null)
        {
            GameManager.Instance.throwingSlider.value = 0f;
            GameManager.Instance.throwingSlider.gameObject.SetActive(false);
            WorldThrowPointer.transform.localScale = PointerScale;
            WorldThrowPointer.SetActive(false);
            line.enabled = false;
        }
        currentHoldTime = 0;
        isCharging = false;

    }

    private void DrawProjection()
    {
        line.enabled = true;
        line.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        Vector3 startPosition = throwOrigin.position;
        float currentThrowForce = Mathf.Lerp(minThrowForce, maxThrowForce, currentCharge);// uses same calculations as the throwing force to accurately predict the path of the item once thrown.
        currentUpwardForce = Mathf.Lerp(upwardForceMax, upwardForceMin, currentCharge);
        Vector3 direction;

        Vector3 aimDirection = transform.forward;

        direction = aimDirection.normalized;
        Vector3 startVelocity = direction * currentThrowForce + Vector3.up.normalized * currentUpwardForce / 1;
        int i = 0;
        line.SetPosition(i, startPosition);
        for (float time = 0; time < linePoints; time += timeBetweenPoints)// draws multiple smaller lines between different points that were calculated to be along the item's throw path
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

    IEnumerator ThrowAnimHandler()// handles the animation of throwing
    {
        animator.SetBool("isChargingThrow", false);
        animator.SetTrigger("Throw");
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("canThrow", false);
    }
    public void resetAnimations() // backup animation reseter
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isFloating", false);
        animator.SetBool("isSprinting", false);
        animator.SetBool("isGrabbing", false);

    }

    private void StartCharging(bool Controller) // handles starting the charging of a throw across both control types
    {
        if (!isCharging)
        {
            animator.SetBool("isChargingThrow", true);
            animator.SetBool("canThrow", true);
        }
        isCharging = true;
        currentCharge = 0f;
        usingController = Controller;
    }
}
