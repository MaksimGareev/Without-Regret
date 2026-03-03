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
    [SerializeField] private float orbitSpeed = 10f;

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

        if (orbitDirection == OrbitDirection.CounterClockwise)
        {
            currentAngle -= Time.deltaTime * orbitSpeed;
        }
        else
        {
            currentAngle += Time.deltaTime * orbitSpeed;
        }

        float x = Mathf.Cos(currentAngle) * radius;
        float z = Mathf.Sin(currentAngle) * radius;

        if (centerPoint != null)
        {
            transform.position = centerPoint.position + new Vector3(x, 0, z);
        }
    }
}
