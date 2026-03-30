using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbitingPlatform : MonoBehaviour
{
    private enum OrbitDirection 
    { 
        Clockwise, 
        CounterClockwise 
    }

    [Header("Orbit Settings")]
    [Tooltip("Direction of orbit for the platform.")]
    [SerializeField] private OrbitDirection orbitDirection = OrbitDirection.Clockwise;

    [Tooltip("Speed at which the platform rotates around the center point.")]
    public float orbitSpeed = 1f;

    [Tooltip("Radius of the circular path the platform follows around the center point.")]
    [SerializeField] private float radius = 5f;

    [Tooltip("The central point around which the platform will rotate.")]
    [SerializeField] private Transform centerPoint;

    [SerializeField, Tooltip("Boolean for if the platform needs an objective to stop orbiting.")] private bool needsObjective;

    [Tooltip("Objective that, when completed, will stop the platform from orbiting.")]
    [SerializeField] private ObjectiveData linkedObjective;

    [Tooltip("Vector position of where the Platform should stop once its marked as objective completed")]
    [SerializeField] private Vector3 stopLocation;
    
    [Tooltip("Used to adjust the angle used in calculating position so that multiple islands don't start in the same place. Set between 0 and 360.")]
    [SerializeField] private float offset;

    private float currentAngle = 0f;
    [SerializeField] private bool objectiveComplete = false;
    [HideInInspector] public bool reachedLocation;
    [SerializeField] private float range = 5f;
    private Rigidbody rb;
    private Vector3 lastPosition;
    public Vector3 platformVelocity { get; private set; }
    private bool lockedOnce = false;

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
        if (centerPoint == null || reachedLocation)
        {
            if (!lockedOnce)
            {
                LockPlatform();
                lockedOnce = true;
            }
            
            return;
        }
            

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
        float x = Mathf.Cos(currentAngle + offset) * radius;
        float z = Mathf.Sin(currentAngle + offset) * radius;

        // Update the platform's position to orbit around the center point
        Vector3 newPosition = centerPoint.position + new Vector3(x, 0, z);
        rb.MovePosition(newPosition);

        platformVelocity = (newPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = newPosition;

        if (objectiveComplete)
        {
            
            Vector3 distOffset = newPosition - stopLocation;
            float squareLength = distOffset.sqrMagnitude;
           // Debug.Log(squareLength);
            float squareRange = range * range;
            if (squareLength <= squareRange)
            {
                reachedLocation = true; Debug.Log("the island has reached its destination");
            }
        }
    }

    private void LockPlatform()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void SetObjectiveComplete(ObjectiveInstance objective)
    {
        if (objective.data == linkedObjective)
        {
            objectiveComplete = true;
        }
    }

    public void SetQTEComplete()
    {
        if (!needsObjective)
        {
            objectiveComplete = true;
            Debug.Log("QTE completed, stopping");
        }
        else
        {
            Debug.Log("Platform is currently set to need a linked objective to be stopped.");
        }
        orbitSpeed = 1;
    }

    private void OnDisable()
    {
        if (linkedObjective != null && ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.OnObjectiveCompleted.RemoveListener(SetObjectiveComplete);
        }
    }
}
