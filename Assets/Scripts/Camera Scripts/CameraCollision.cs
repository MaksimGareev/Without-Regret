using UnityEngine;

public class CameraCollider : MonoBehaviour
{
    [Header("Target (player)")]
    public Transform target;

    [Header("Collision Settings")]
    public LayerMask collisionMask;
    public float sphereRadius = 0.2f;
    public float minDistance = 0.3f;

    private float currentDistance;

    void LateUpdate()
    {
        if (target == null) return;

        // Direction from target to current camera position (from your existing script)
        Vector3 direction = (transform.position - target.position);
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit hit;

        // Check for obstacles
        if (Physics.SphereCast(target.position, sphereRadius, direction, out hit, distance, collisionMask))
        {
            // Move camera in front of the obstacle
            float safeDistance = Mathf.Max(hit.distance, minDistance);
            transform.position = target.position + direction * safeDistance;
        }
        // else: DO NOTHING → let your existing camera script control position
    }
}