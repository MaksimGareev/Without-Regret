using UnityEngine;

public class PlayerMovingObjects : MonoBehaviour
{
    public Animator animator;
    [Header("General Settings")]
    [SerializeField] public Transform grabPoint;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;
    
    private PlayerController playerController;
    private float normalMoveSpeed;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    private void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        normalMoveSpeed = playerController.Speed;
    }

    public void OnMovingObject(float moveSlowdownMult)
    {
        if (animator != null)
        {
            resetAnimations();
            animator.SetBool("isGrabbing", true); //enter grabbing anim state
        }
        
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

        if (animator != null)
        {
            resetAnimations(); //exit animation state
        }
    }

    private void resetAnimations()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isGrabbing", false);
        animator.SetBool("isFloating", false);
        animator.SetBool("isPulling", false);
        animator.SetBool("isPushing", false);
    }

}
