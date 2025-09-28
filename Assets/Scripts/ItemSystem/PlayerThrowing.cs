using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

public class PlayerThrowing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Slider powerSlider;
    
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
    }

    // Update is called once per frame
    private void Update()
    {
        HandleCharging();
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
                    }
                }
            }
        }
    }

    private void ThrowItem(float normalizedPower, bool usingController)
    {
        ItemData itemToThrow = inventory.GetFirstThrowable();

        if (itemToThrow == null) return;

        inventory.RemoveItem(itemToThrow);

        GameObject gameObject = Instantiate(itemToThrow.Prefab, throwOrigin.position, Quaternion.identity);

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction;

            if (usingController)
            {
                float RjoystickX = -Input.GetAxis("Xbox RightStick X");
                float RjoystickY = Input.GetAxis("Xbox RightStick Y");

                Vector3 aimDirection = new Vector3(RjoystickX, 0, RjoystickY);

                if (aimDirection.magnitude < 0.01f)
                {
                    aimDirection = transform.forward;
                }

                direction = aimDirection.normalized;
            }
            else
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 targetPoint;

                if (Physics.Raycast(ray, out RaycastHit hit))
                    targetPoint = hit.point;
                else
                    targetPoint = ray.GetPoint(10f);

                direction = (targetPoint - transform.position).normalized;
            }

            float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, normalizedPower);
            rb.AddForce(direction * throwForce + Vector3.up * upwardForce, ForceMode.Impulse);
        }
    }
}
