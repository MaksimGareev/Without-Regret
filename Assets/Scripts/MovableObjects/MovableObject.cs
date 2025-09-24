using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour
{
    [SerializeField] private Transform grabPoint;
    private Rigidbody rb;

    public Rigidbody Rigidbody => rb;
    public Transform GrabPoint => grabPoint != null ? grabPoint : transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
