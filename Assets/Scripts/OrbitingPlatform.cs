using UnityEngine;

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
    
    [Tooltip("Used to adjust the angle used in calculating position so that multiple islands don't start in the same place. Set between 0 and 360.")]
    [SerializeField] private float offset;

    [Tooltip("Used to adjust the starting y-position of the island")]
    [SerializeField] private float height;

   

    private float currentAngle = 0f;
    private bool objectiveComplete = false;

    private void Awake()
    {
        if (centerPoint == null)
        {
            Debug.LogError("Center Point is not assigned for OrbitingPlatform.");
        }
       
        
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

    private void LateUpdate()
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
        float x = Mathf.Cos(currentAngle + offset) * radius;
        float z = Mathf.Sin(currentAngle + offset) * radius;

        // Update the platform's position to orbit around the center point
        transform.position = centerPoint.position + new Vector3(x, height, z);
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
