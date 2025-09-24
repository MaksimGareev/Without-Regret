using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour, IMoveable
{
    private Transform grabPoint;
    private Rigidbody rb;
    private bool isGrabbed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Grab(Transform grabPoint)
    {
        this.grabPoint = grabPoint;
        isGrabbed = true;

        rb.isKinematic = true;
    }

    public void Release()
    {
        isGrabbed = false;
        grabPoint = null;

        rb.isKinematic = false;
    }

    private void FixedUpdate()
    {
        if (isGrabbed && grabPoint != null)
        {
            rb.MovePosition(grabPoint.position);
            rb.MoveRotation(grabPoint.rotation);
        }
    }
}
