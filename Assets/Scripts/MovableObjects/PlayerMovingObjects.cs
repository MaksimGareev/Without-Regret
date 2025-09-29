using UnityEngine;

public class PlayerMovingObjects : MonoBehaviour
{
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabRange = 2f;
    [SerializeField] private KeyCode grabKey = KeyCode.E;
    [SerializeField] private string grabButton = "Xbox X Button";
    
    private PlayerController playerController;
    private float normalMoveSpeed;
    private IMoveable heldObject;

    private void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
    }

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
                normalMoveSpeed = playerController.Speed;
                playerController.Speed = normalMoveSpeed / 3;
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
            playerController.Speed = normalMoveSpeed;
        }
    }
}
