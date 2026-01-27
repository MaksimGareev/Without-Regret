using UnityEngine;

public class PlayerMovingObjects : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] public Transform grabPoint;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    
    private PlayerController playerController;
    private float normalMoveSpeed;

    private void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        normalMoveSpeed = playerController.Speed;
    }

    public void OnMovingObject(float moveSlowdownMult)
    {
        normalMoveSpeed = playerController.Speed;
        playerController.Speed = normalMoveSpeed / moveSlowdownMult;
        playerController.SetCanSprint(false);

        if (playerController.animator != null)
        {
            playerController.animator.SetBool("isIdle", false);
            playerController.animator.SetBool("isWalking", false);
            playerController.animator.SetBool("isGrabbing", true);
        }
        

        if (showDebugLogs)
        {
            Debug.Log($"Grabbed");
        }
    }

    public void OnReleaseObject()
    {
        if (playerController.animator != null)
            playerController.animator.SetBool("isGrabbing", false);
        playerController.Speed = normalMoveSpeed;
        playerController.SetCanSprint(true);
    }
}
