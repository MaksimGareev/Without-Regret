using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private float smoothSpeed = 0.01f;
    public Vector3 offset = new Vector3(0, 0, -10);
    private Transform target;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("CameraFollow: No GameObject with tag 'Player' found in the scene!");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
