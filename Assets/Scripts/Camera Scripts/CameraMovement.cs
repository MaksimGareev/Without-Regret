using System.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public enum WorldDirection
    {
        North,
        South,
        East,
        West
    }

    [Header("Follow Settings")]
    public Transform target; // Reference the player as the intended target of the camera
    public Vector3 defaultOffset = new Vector3(0, 8, 8); // Height and distance away from the player
    public Vector3 defaultLookAtOffset = Vector3.zero;
    public float smoothSpeed = 5f; // Speed the camera moves to follow the player

    [Header("Default Facing Direction")]
    [Tooltip("Defines the world-space direction that the camera should face by default.")]
    [SerializeField] WorldDirection defaultFacingDirection = WorldDirection.North;

    [Header("Camera Control Settings")]
    [SerializeField] private bool rotateCamera = false;
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float maxYaw = 120f;
    [SerializeField] private float translateSpeed = 5f;
    [SerializeField] private float translateLimit = 4f;
    [SerializeField] private float returnSpeed = 4f;
    [SerializeField] private float mouseResetTime = 3f;

    [Header("Focus Movement Settings")]
    public Vector3 pickupOffset = new Vector3(3f, 2f, -5f);
    public float zoomDuration = 2f;
    public float transitionSpeed = 2f;
    private bool isZooming = false;

    private Vector3 currentOffset;
    private Vector3 currentLookAtOffset;
    private float yaw;
    private float pitch;
    private Quaternion initialRotation;
    private PlayerThrowing playerThrowing;
    private bool isThrowing;
    private ToggleInventoryUI toggleInventoryUI;
    private float mouseResetTimer;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target not assigned!");
            enabled = false;
            return;
        }

        playerThrowing = target.GetComponent<PlayerThrowing>();
        toggleInventoryUI = target.GetComponent<ToggleInventoryUI>();

        Vector3 facingVector = DirectionToVector(defaultFacingDirection);
        initialRotation = Quaternion.LookRotation(facingVector, Vector3.up);

        currentOffset = initialRotation * defaultOffset;
        currentLookAtOffset = initialRotation * defaultLookAtOffset;

        yaw = 0f;
        pitch = 0f;

        transform.position = target.position + currentOffset;
        transform.LookAt(target.position + currentLookAtOffset);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private Vector3 DirectionToVector(WorldDirection direction)
    {
        switch (direction)
        {
            case WorldDirection.North:
                return Vector3.back;
            case WorldDirection.South:
                return Vector3.forward;
            case WorldDirection.East:
                return Vector3.left;
            case WorldDirection.West:
                return Vector3.right;
            default:
                return Vector3.forward;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return;

        PlayerController pc = target.GetComponent<PlayerController>();
        if (pc != null)
        {
            if (pc.MovementLocked == true)
            {
                pc.enabled = false;
            }
        }

        float horizontalInput = Input.GetAxis("Xbox RightStick X");
        float verticalInput = Input.GetAxis("Xbox RightStick Y");

        float mouseX = -Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        bool hasControllerInput = Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;
        bool hasMouseInput = Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f;

        if (playerThrowing != null)
        {
            isThrowing = playerThrowing.GetIsCharging();
        }

        if (!hasMouseInput && mouseResetTimer >= 0)
        {
            mouseResetTimer -= Time.deltaTime;
        }

        if (!isZooming)
        {
            if (rotateCamera)
            {
                if (hasControllerInput && !isThrowing && !toggleInventoryUI.isEnabled && !pc.MovementLocked)
                {
                    HandleRotation(horizontalInput, verticalInput);
                }
                else if (hasMouseInput && !isThrowing && !toggleInventoryUI.isEnabled && !pc.MovementLocked)
                {
                    HandleRotation(mouseX, mouseY);
                    mouseResetTimer = mouseResetTime;
                }
                else
                {
                    ReturnRotation();
                }
            
            }
            else
            {
                if (hasControllerInput && !isThrowing && !toggleInventoryUI.isEnabled && !pc.MovementLocked)
                {
                    HandleTranslation(horizontalInput, verticalInput);
                }
                else if (hasMouseInput && !isThrowing && !toggleInventoryUI.isEnabled && !pc.MovementLocked)
                {
                    HandleTranslation(mouseX, mouseY);
                    mouseResetTimer = mouseResetTime;
                }
                else
                {
                    ReturnTranslation();
                }
            }

            // Position of the camera
            Vector3 desiredPosition = target.position + currentOffset;

            // Smooth following of the player
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            Vector3 lookAtPos = target.position + currentLookAtOffset;

            // Look at the Player
            transform.LookAt(lookAtPos);
        } 
    }

    private void HandleRotation(float horizontalInput, float verticalInput)
    {
        yaw -= horizontalInput * rotateSpeed * Time.deltaTime;
        pitch -= verticalInput * rotateSpeed * Time.deltaTime;

        yaw = Mathf.Clamp(yaw, -Mathf.Abs(maxYaw), Mathf.Abs(maxYaw));
        pitch = Mathf.Clamp(pitch, -Mathf.Abs(maxPitch), Mathf.Abs(maxPitch));

        Quaternion rotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);

        currentOffset = rotation * defaultOffset;

        currentLookAtOffset = defaultLookAtOffset;
    }
    
    private void ReturnRotation()
    {
        if (mouseResetTimer >= 0f)
        {
            return;
        }

        yaw = Mathf.Lerp(yaw, 0f, returnSpeed * Time.deltaTime);
        pitch = Mathf.Lerp(pitch, 0f, returnSpeed * Time.deltaTime);

        Quaternion rotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);
        currentOffset = rotation * defaultOffset;

        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, initialRotation * defaultLookAtOffset, returnSpeed * Time.deltaTime);
    }

    private void HandleTranslation(float horizontalInput, float verticalInput)
    {
        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput);
        Vector3 delta = initialRotation * inputDirection * translateSpeed * Time.deltaTime;

        currentOffset += delta;
        currentLookAtOffset += delta;

        Vector3 offsetFromDefault = currentOffset - (initialRotation * defaultOffset);

        if (offsetFromDefault.magnitude > translateLimit)
        {
            offsetFromDefault = offsetFromDefault.normalized * translateLimit;
            currentOffset = initialRotation * defaultOffset + offsetFromDefault;
            currentLookAtOffset = initialRotation * defaultLookAtOffset + offsetFromDefault;
        }
    }
    
    private void ReturnTranslation()
    {
        currentOffset = Vector3.Lerp(currentOffset, initialRotation * defaultOffset, returnSpeed * Time.deltaTime);
        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, initialRotation * defaultLookAtOffset, returnSpeed * Time.deltaTime);
    }

    public void TriggerDialogueCamera(Transform dialogueTrigger)
    {
        if (!isZooming)
        {
            StartCoroutine(ZoomIntoTarget(dialogueTrigger, true));
        }
    }

    public void EndDialogueCamera()
    {
        if (isZooming)
        {
            isZooming = false;
        }
    }

    public void TriggerPickupCameraEffect(Transform item)
    {
        if (!isZooming)
        {
            StartCoroutine(ZoomIntoTarget(item, false));
            StartCoroutine(PauseZoomForItem());
        }
    }

    IEnumerator PauseZoomForItem()
    {
        yield return new WaitForSeconds(zoomDuration / 2f);
        isZooming = false;
    }

    public IEnumerator ZoomIntoTarget(Transform target, bool dialogue = false)
    {
        isZooming = true;
        Vector3 offset;

        if (dialogue)
        {
            offset = target.right * 1f + target.forward * 5f + Vector3.up * 2f;
        }
        else 
        {
            offset = pickupOffset;
        } 

        Vector3 originalCamPos = transform.position;
        Quaternion originalCamRot = transform.rotation;

        Vector3 targetPos = target.position + (transform.forward * 1f) + offset;
        Quaternion targetRot = Quaternion.LookRotation(target.position - transform.position);

        Vector3 lookAtPos = target.position;

        float t = 0;
        while (t < zoomDuration)
        {
            t += Time.deltaTime * transitionSpeed;
            transform.position = Vector3.Lerp(originalCamPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(originalCamRot, targetRot, t);
            transform.LookAt(lookAtPos);
            yield return null;
        }
    }
}
