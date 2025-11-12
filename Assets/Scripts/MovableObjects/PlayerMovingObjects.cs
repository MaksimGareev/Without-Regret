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
    }

    public void OnMovingObject(float moveSlowdownMult)
    {
        normalMoveSpeed = playerController.Speed;
        playerController.Speed = normalMoveSpeed / moveSlowdownMult;
        playerController.SetCanSprint(false);
        
        if (showDebugLogs)
        {
            Debug.Log($"Grabbed");
        }
    }

    public void OnReleaseObject()
    {
        playerController.Speed = normalMoveSpeed;
        playerController.SetCanSprint(true);
    }
}
