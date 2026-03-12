using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraOverrideTrigger : MonoBehaviour
{
    [Header("Camera Override Settings")]
    [Tooltip("The world direction that the camera will be oriented towards when inside the trigger. This will affect the direction of the overrideOffset as well.")]
    [SerializeField] private CameraMovement.WorldDirection worldDirection = CameraMovement.WorldDirection.North;

    [Tooltip("Offset of the camera from the player when inside the trigger.")]
    [SerializeField] private Vector3 overrideOffset = new Vector3(0, 8, 8);

    [Tooltip("Offset applied to the camera's look at position when inside the trigger.")]
    [SerializeField] private Vector3 overrideLookAtOffset = new Vector3(0, 3, 0);

    [Tooltip("Duration of the transition when the camera changes to the override position and rotation.")]
    [SerializeField, Min(0.0f)] private float transitionDuration = 1.0f;

    [Tooltip("If true, the camera will follow the player. If false, the camera will remain in the position it is placed in the editor, or the position it is at the time this boolean is set false.")]
    [SerializeField] private bool followPlayer = true;

    [Tooltip("Speed at which the camera moves to follow the player when inside the trigger.")]
    [SerializeField, Range(0.0f, 10.0f)] private float smoothSpeed = 5.0f;

    [Tooltip("Whether or not the player is allowed to rotate the camera while inside the trigger.")]
    [SerializeField] private bool rotateCamera = true;

    [Tooltip("Speed at which the camera rotates when rotateCamera is enabled.")]
    [SerializeField, Range(0.0f, 180.0f)] private float rotateSpeed = 120.0f;

    [Tooltip("If true, the camera's yaw will be restricted to the maxYaw angle. If false, the camera can rotate freely.")]
    [SerializeField] private bool restrictYaw = false;

    [Tooltip("The maximum yaw angle the camera can rotate to while inside the trigger.")]
    [SerializeField, Range(0.0f, 180.0f)] private float maxYaw = 120.0f;

    [Tooltip("The maximum pitch angle the camera can rotate to while inside the trigger.")]
    [SerializeField, Range(0.0f, 90.0f)] private float maxPitch = 45.0f;

    private CameraMovement cam;
    private GameObject player;

    private void Awake()
    {
        cam = Camera.main.GetComponent<CameraMovement>();
        if (cam == null)
        {
            Debug.LogError("CameraMovement script not found on the main camera. Please ensure the main camera has a CameraMovement component.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found in the scene. Please ensure there is a GameObject with the tag 'Player'.");
        }

        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("No Collider component found on the CameraOverrideTrigger object. Please add a Collider (e.g., BoxCollider) and set it as a trigger.");
        }
        else if (!GetComponent<Collider>().isTrigger)
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player || cam == null) return;

        cam.OverrideCameraPosition(worldDirection, overrideOffset, overrideLookAtOffset, transitionDuration);
        cam.OverrideFollowSettings(followPlayer, smoothSpeed);
        cam.OverrideRotationSettings(rotateCamera, rotateSpeed, restrictYaw, maxYaw, maxPitch);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != player || cam == null) return;

        cam.ResetCameraPosition();
        cam.ResetFollowSettings();
        cam.ResetRotationSettings();
    }
}
