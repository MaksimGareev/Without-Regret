using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbitingPlatform : MonoBehaviour
{
    public enum OrbitDirection 
    { 
        Clockwise, 
        CounterClockwise 
    }

    [Header("Orbit Settings")]
    [Tooltip("Direction of orbit for the platform.")]
    [SerializeField] private OrbitDirection orbitDirection = OrbitDirection.Clockwise;

    [Tooltip("Speed at which the platform rotates around the center point.")]
    [SerializeField] private float orbitSpeed = 1f;

    [Tooltip("Radius of the circular path the platform follows around the center point.")]
    [SerializeField] private float radius = 5f;

    [Tooltip("The central point around which the platform will rotate.")]
    [SerializeField] private Transform centerPoint;

    [Tooltip("Objective that, when completed, will stop the platform from orbiting.")]
    [SerializeField] private ObjectiveData linkedObjective;

    private float currentAngle = 0f;
    private bool objectiveComplete = false;
    private Rigidbody rb;
    private Vector3 lastPosition;
    public Vector3 platformVelocity { get; private set; }

    private void Awake()
    {
        if (centerPoint == null)
        {
            Debug.LogError("Center Point is not assigned for OrbitingPlatform.");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on the OrbitingPlatform.");
        }

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        lastPosition = transform.position;
    }

    private void OnEnable()
    {
        if (linkedObjective != null && ObjectiveManager.Instance != null)
        {
            if (ObjectiveManager.Instance.IsObjectiveCompleted(linkedObjective.objectiveID))
            {
                objectiveComplete = true;
            }
            else
            {
                ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveComplete);
            }
        }
    }

    private void FixedUpdate()
    {
        if (centerPoint == null || objectiveComplete) return;

        // Calculate the new angle based on the orbit direction and speed
        if (orbitDirection == OrbitDirection.CounterClockwise)
        {
            // Decrease the angle for counter-clockwise rotation
            currentAngle += Time.deltaTime * orbitSpeed;
        }
        else
        {
            // Increase the angle for clockwise rotation
            currentAngle -= Time.deltaTime * orbitSpeed;
        }

        // Keep the angle within the range of 0 to 360 degrees
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }
        else if (currentAngle < 0f)
        {
            currentAngle += 360f;
        }

        // Calculate the new position of the platform based on the current angle and radius
        float x = Mathf.Cos(currentAngle) * radius;
        float z = Mathf.Sin(currentAngle) * radius;

        // Update the platform's position to orbit around the center point
        Vector3 newPosition = centerPoint.position + new Vector3(x, 0, z);
        rb.MovePosition(newPosition);

        platformVelocity = (newPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = newPosition;
    }

    private void SetObjectiveComplete(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            objectiveComplete = true;
        }
    }

    private void OnDisable()
    {
        if (linkedObjective != null && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(SetObjectiveComplete);
        }
    }
}
