using UnityEngine;

public class PlayerMovingObjects : MonoBehaviour
{
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabRange = 2f;
    [SerializeField] private KeyCode grabKey = KeyCode.E;
    [SerializeField] private string grabButton = "Xbox X Button";

    private IMoveable heldObject;

    private void Update()
    {
        if (Input.GetKeyDown(grabKey) || Input.GetButtonDown(grabButton))
        {
            if (heldObject == null)
            {
                TryGrab();
            }
            else
            {
                Release();
            }
        }
    }

    private void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);
        foreach (Collider hit in hits)
        {
            IMoveable moveable = hit.GetComponent<IMoveable>();

            if (moveable != null)
            {
                moveable.Grab(grabPoint);
                heldObject = moveable;
                Debug.Log($"Grabbed");
            }
        }
    }

    private void Release()
    {
        if (heldObject != null)
        {
            heldObject.Release();
            heldObject = null;
        }
    }
}
