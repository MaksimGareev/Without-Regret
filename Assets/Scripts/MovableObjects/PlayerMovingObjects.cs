using UnityEngine;

public class PlayerMovingObjects : MonoBehaviour
{
    [SerializeField] private float grabRange = 2f;
    [SerializeField] private KeyCode grabKey = KeyCode.E;
    [SerializeField] private Transform playerHand;
    [SerializeField] private float moveForce = 50f;

    private MoveableObject heldObject;
    private FixedJoint joint;

    private void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            if (heldObject == null)
                TryGrab();
            else
                Release();
        }
    }

    private void FixedUpdate()
    {
        if (heldObject != null)
        {
            Vector3 targetPos = playerHand.position;
            Vector3 force = (targetPos - heldObject.transform.position) * moveForce;
            heldObject.Rigidbody.AddForce(force, ForceMode.Force);
        }
    }

    private void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);
        foreach (Collider hit in hits)
        {
            MoveableObject moveable = hit.GetComponent<MoveableObject>();
            if (moveable != null)
            {
                heldObject = moveable;
                Debug.Log($"Grabbed {moveable.name}");
                return;
            }
        }
    }

    private void Release()
    {
        Debug.Log($"Released {heldObject.name}");
        heldObject = null;
    }
}
