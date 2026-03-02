using UnityEngine;

public class OrbitingPlatform : MonoBehaviour
{
    public enum RotationDirection 
    { 
        Clockwise, 
        CounterClockwise 
    }

    [Header("Rotation Settings")]
    [Tooltip("Direction of rotation for the platform.")]
    [SerializeField] private RotationDirection rotationDirection = RotationDirection.Clockwise;

    [Tooltip("Speed at which the platform rotates around the center point.")]
    [SerializeField] private float rotateSpeed = 10f;

    [Tooltip("Radius of the circular path the platform follows around the center point.")]
    [SerializeField] private float radius = 5f;

    [Tooltip("The central point around which the platform will rotate.")]
    [SerializeField] private Transform centerPoint;
    private float currentAngle = 0f;

    private void Start()
    {
        if (centerPoint == null)
        {
            Debug.LogError("Center Point is not assigned for OrbitingPlatform.");
        }
    }

    private void LateUpdate()
    {
        if (centerPoint == null) return;

        if (rotationDirection == RotationDirection.CounterClockwise)
        {
            currentAngle -= Time.deltaTime * rotateSpeed;
        }
        else
        {
            currentAngle += Time.deltaTime * rotateSpeed;
        }

        float x = Mathf.Cos(currentAngle) * radius;
        float z = Mathf.Sin(currentAngle) * radius;

        if (centerPoint != null)
        {
            transform.position = centerPoint.position + new Vector3(x, 0, z);
        }
    }
}
