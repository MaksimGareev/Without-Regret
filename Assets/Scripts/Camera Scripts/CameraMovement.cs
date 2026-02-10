using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEditor;
//using UnityEditor.EditorTools;

public class CameraMovement : MonoBehaviour
{
    public enum WorldDirection
    {
        North,
        South,
        East,
        West
    }

    [Header("General Settings")]
    [Tooltip("Set to true if this camera is used in the astral plane for special effects.")]
    public bool isAstral = false;

    [Tooltip("Set to true if this camera is used indoors.")]
    public bool isIndoors = false;

    [Header("Input")]
    [Tooltip("Insert a reference to the PlayerControls Input Action Asset")]
    [SerializeField] private InputActionAsset inputActions;
    private string actionMapName = "Player";
    private string lookActionName = "Look";
    private InputAction lookAction;

    [Header("Follow Settings")]
    private Transform target; // Reference the player as the intended target of the camera
    public Vector3 defaultOffset = new Vector3(0, 8, 8); // Height and distance away from the player
    public Vector3 defaultLookAtOffset = Vector3.zero;
    public float smoothSpeed = 5f; // Speed the camera moves to follow the player

    [Header("Default Facing Direction")]
    [Tooltip("Defines the world-space direction that the camera should face by default.")]
    [SerializeField] WorldDirection defaultFacingDirection = WorldDirection.North;

    [Header("Camera Control Settings")]
    private bool rotateCamera = true;
    private bool restrictYaw = false;
    [Tooltip("Speed at which the camera rotates.")]
    [SerializeField] private float rotateSpeed = 120f;
    [Tooltip("Maximum pitch angle of the camera.")]
    [SerializeField] private float maxPitch = 45f;
    private float maxYaw = 120f;
    [Tooltip("Speed at which the camera returns to its default position.")]
    [SerializeField] private float returnSpeed = 4f;
    [Tooltip("Time in seconds after last mouse input before the camera starts returning to default.")]
    [SerializeField] private float mouseResetTime = 3f;
    [Tooltip("Scale factor for mouse rotation sensitivity.")]
    [SerializeField] private float mouseRotateScale = 0.08f;

    [Header("Focus Movement Settings")]
    [Tooltip("Offset applied to the camera when focusing on a pickup object.")]
    public Vector3 pickupOffset = new Vector3(3f, 2f, -5f);
    [Tooltip("Duration of the zoom effect when focusing on a pickup.")]
    public float zoomDuration = 2f;
    [Tooltip("Speed at which the camera transitions during focus movement.")]
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
    //private PlayerThrowing playerThrowing;
    //private bool isThrowing;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        lookAction?.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (target == null)
        {
            Debug.LogWarning("Camera target not assigned, and no GameObject with tag 'Player' found. Disabling CameraMovement.");
            enabled = false;
            return;
        }

        //playerThrowing = target.GetComponent<PlayerThrowing>();
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

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Destroy(this);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        CameraLocked = false;
        isZooming = false;
        yaw = 0f;
        pitch = 0f;

       // cameraPivot.localRotation = Quaternion.identity;
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

        if (lookInput.sqrMagnitude > 0.0001f && lookAction != null && lookAction.activeControl.device is Mouse)
        {
            lastInputWasMouse = true;
        }
        else
        {
            lastInputWasMouse = false;
        }

        bool hasLookInput = lookInput.sqrMagnitude > 0.0001f;

        //if (playerThrowing != null)
        //{
        //    isThrowing = playerThrowing.GetIsCharging();
        //}

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
            float stickScale = GameSettings.RightStickSensitivity;
            yaw -= horizontalInput * rotateSpeed * stickScale * Time.deltaTime;
            pitch -= verticalInput * rotateSpeed * stickScale * Time.deltaTime;
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

        currentLookAtOffset = rotation * defaultLookAtOffset;
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

        // TO FIX: With this implementation, the camera will snap back to the original position and rotation at the end of the coroutine
        // Because LateUpdate will override the position and rotation once it finishes.
        // Commenting this out for now so it's a little less jarring, but a smooth transition would be ideal later

        //transform.GetPositionAndRotation(out Vector3 currentPos, out Quaternion currentRot);
        //float t = 0;
        //while (t < zoomDuration)
        //{
        //    t += Time.deltaTime * transitionSpeed;
        //    transform.SetPositionAndRotation(Vector3.Lerp(currentPos, camPosCache, t), Quaternion.Slerp(currentRot, camRotCache, t));
        //    Vector3 lookAtPos = Vector3.Lerp(lookAtCache, target.position + currentLookAtOffset, t);
        //    transform.LookAt(lookAtPos);
        //    yield return null;
        //}

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

        transform.SetPositionAndRotation(targetPos, targetRot);
    }

    public void SetCameraLocked(bool locked)
    {
        CameraLocked = locked;
    }

}
