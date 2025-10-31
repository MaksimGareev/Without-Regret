using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour, IInteractable
{
    private PlayerMovingObjects playerMovingObjects; 
    private Transform grabPoint;
    private Rigidbody rb;
    private bool isGrabbed = false;
    public float interactionPriority => 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    private void Grab(Transform grabPoint)
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

    public void OnPlayerInteraction(GameObject player)
    {
        playerMovingObjects = player.GetComponent<PlayerMovingObjects>();

        if (!isGrabbed)
        {
            Grab(playerMovingObjects.grabPoint);
            playerMovingObjects.OnMovingObject();
        }
        else
        {
            Release();
            playerMovingObjects.OnReleaseObject();
        }
    }
}
