using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public enum WorldDirection
    {
        North,
        South,
        East,
        West
    }
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string lookActionName = "Look";
    private InputAction lookAction;

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
    [SerializeField] private bool restrictYaw = false;
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float maxYaw = 120f;
    [SerializeField] private float translateSpeed = 5f;
    [SerializeField] private float translateLimit = 4f;
    [SerializeField] private float returnSpeed = 4f;
    [SerializeField] private float mouseResetTime = 3f;
    [SerializeField] private float mouseRotateScale = 0.08f;
    [SerializeField] private float mouseTranslateScale = 0.01f;

    [Header("Focus Movement Settings")]
    public Vector3 pickupOffset = new Vector3(3f, 2f, -5f);
    public float zoomDuration = 2f;
    public float transitionSpeed = 2f;
    private bool isZooming = false;
    private Vector3 camPosCache = Vector3.zero;
    private Quaternion camRotCache = Quaternion.identity;
    private Vector3 lookAtCache = Vector3.zero;
    public bool CameraLocked { get; private set; }
    //public Transform player;

    private Vector3 currentOffset;
    private Vector3 currentLookAtOffset;
    private float yaw;
    private float pitch;
    private Quaternion initialRotation;
    private PlayerThrowing playerThrowing;
    private bool isThrowing;
    private ToggleInventoryUI toggleInventoryUI;
    private float mouseResetTimer;
    private bool lastInputWasMouse = false;

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap(actionMapName, true);
            lookAction = map.FindAction(lookActionName, true);
        }
        else
        {
            Debug.LogWarning("InputActionAsset not assigned in CameraMovement.");
        }
    }

    private void OnEnable()
    {
        lookAction?.Enable();
    }

    private void OnDisable()
    {
        lookAction?.Disable();
    }

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CameraLocked = false;
        isZooming = false;
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
        if (CameraLocked) return;
        if (target == null) return;

        PlayerController pc = target.GetComponent<PlayerController>();

        if (pc != null && pc.MovementLocked && pc.enabled)
        {
            pc.enabled = false;
        }

        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        if (lookInput.sqrMagnitude > 0.0001f && lookAction != null && lookAction.activeControl != null)
        {
            lastInputWasMouse = true;
        }

        bool hasLookInput = lookInput.sqrMagnitude > 0.0001f;

        if (playerThrowing != null)
        {
            isThrowing = playerThrowing.GetIsCharging();
        }

        if (!hasLookInput && mouseResetTimer >= 0)
        {
            mouseResetTimer -= Time.deltaTime;
        }

        bool blocked = isThrowing || (toggleInventoryUI != null && toggleInventoryUI.isEnabled) || (pc != null && pc.MovementLocked);

        if (!isZooming)
        {
            if (!blocked && hasLookInput)
            {
                float h = -lookInput.x;
                float v = -lookInput.y;

                if (rotateCamera)
                {
                    HandleRotation(h, v, lastInputWasMouse);
                }
                else
                {
                    HandleTranslation(h, v, lastInputWasMouse);
                }

                if (lastInputWasMouse)
                {
                    mouseResetTimer = mouseResetTime;
                }
            }
            else
            {
                if (rotateCamera)
                {
                    ReturnRotation();
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

    private void HandleRotation(float horizontalInput, float verticalInput, bool isMouse)
    {
        if (isMouse)
        {
            float mouseScale = mouseRotateScale * GameSettings.MouseSensitivity;
            yaw -= horizontalInput * mouseScale;
            pitch -= verticalInput * mouseScale;
        }
        else
        {
            float stickScale = rotateSpeed * GameSettings.RightStickSensitivity;
            yaw -= horizontalInput * stickScale * Time.deltaTime;
            pitch -= verticalInput * stickScale * Time.deltaTime;
        }

        if (restrictYaw)
        {
            yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        }
        else
        {
            yaw = Mathf.Repeat(yaw + 180f, 360f) - 180f;
        }

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

        if (restrictYaw)
        {
            yaw = Mathf.Lerp(yaw, 0f, returnSpeed * Time.deltaTime);
        }
        
        pitch = Mathf.Lerp(pitch, 0f, returnSpeed * Time.deltaTime);

        Quaternion rotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);
        currentOffset = rotation * defaultOffset;

        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, initialRotation * defaultLookAtOffset, returnSpeed * Time.deltaTime);
    }

    private void HandleTranslation(float horizontalInput, float verticalInput, bool isMouse)
    {
        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput);

        float scale;
        if (isMouse)
        {
            scale = mouseTranslateScale * GameSettings.MouseSensitivity;
        }
        else
        {
            scale = translateSpeed * GameSettings.RightStickSensitivity * Time.deltaTime;
        }

        Vector3 delta = initialRotation * inputDirection * scale;

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
            StartCoroutine(StartCameraZoom(dialogueTrigger, true));
        }
    }

    public IEnumerator EndCameraZoom()
    {
        if (camPosCache == Vector3.zero || camRotCache == Quaternion.identity || lookAtCache == Vector3.zero)
        {
            yield break;
        }

        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        float t = 0;
        while (t < zoomDuration)
        {
            t += Time.deltaTime * transitionSpeed;
            transform.position = Vector3.Lerp(currentPos, camPosCache, t);
            transform.rotation = Quaternion.Slerp(currentRot, camRotCache, t);
            Vector3 lookAtPos = Vector3.Lerp(lookAtCache, target.position + currentLookAtOffset, t);
            transform.LookAt(lookAtPos);
            yield return null;
        }

        isZooming = false;
        CameraLocked = false;

        camPosCache = Vector3.zero;
        camRotCache = Quaternion.identity;
        lookAtCache = Vector3.zero;
    }

    public void TriggerPickupCameraEffect(Transform item)
    {
        if (!isZooming)
        {
            StartCoroutine(StartCameraZoom(item, false));
            StartCoroutine(PauseZoomForItem());
        }
    }

    IEnumerator PauseZoomForItem()
    {
        yield return new WaitForSeconds(zoomDuration / 2f);
        StartCoroutine(EndCameraZoom());
    }

    public IEnumerator StartCameraZoom(Transform zoomTarget, bool dialogue = false)
    {
        CameraLocked = true;
        isZooming = true;

        // Cashe current camera transform
        camPosCache = transform.position;
        camRotCache = transform.rotation;

        // Direction the player is facing
        Vector3 playerForward = zoomTarget.position - target.position;
        playerForward.y = 0f;
        playerForward.Normalize();

        // Offset relative to the player
        Vector3 offset;
        if (dialogue)
        {
            // Offset behind player, up, and to the right
            offset = -playerForward * 3f + target.right * 3f + Vector3.up * 2f;
        }
        else
        {
            offset = pickupOffset;
        }

        // Target position
        Vector3 targetPos = target.position + offset;

        // Target Rotation
        Quaternion targetRot = Quaternion.LookRotation(zoomTarget.position - targetPos);

        lookAtCache = zoomTarget.position;

        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime * transitionSpeed;
            transform.position = Vector3.Lerp(camPosCache, targetPos, t);
            transform.rotation = Quaternion.Slerp(camRotCache, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    public void SetCameraLocked(bool locked)
    {
        CameraLocked = locked;
    }

}
