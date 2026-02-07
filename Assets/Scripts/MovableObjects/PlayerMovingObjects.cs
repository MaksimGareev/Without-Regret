using System.Collections.Generic;
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
    private float normalSprintSpeed;
    private HashSet<MoveableObject> movedObjects = new HashSet<MoveableObject>();

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = gameObject.GetComponent<PlayerController>();
        normalMoveSpeed = playerController.Speed;
        normalSprintSpeed = playerController.SprintSpeed;
    }

    public void OnMovingObject(MoveableObject obj)
    {
        // Return early if already occupied or object is already being moved
        if (IsOccupied()) return;
        if (!movedObjects.Add(obj)) return;

        if (movedObjects.Count > 1 && showDebugLogs)
        {
            Debug.LogError($"Attempted to move {movedObjects.Count} objects at the same time");
            obj.Release();
            OnReleaseObject(obj);
            return;
        }

        if (showDebugLogs)
            Debug.Log($"OnMovingObject called for {obj.gameObject.name}, occupied status: {IsOccupied()}");
        

        if (animator != null)
        {
            resetAnimations();
            animator.SetBool("isGrabbing", true); //enter grabbing anim state
        }
        
        normalMoveSpeed = playerController.Speed;
        normalSprintSpeed = playerController.SprintSpeed;
        playerController.Speed = normalMoveSpeed / obj.GetMoveSlowdown();
        playerController.SprintSpeed = normalSprintSpeed / obj.GetSprintSlowdown();
        playerController.MovingObject(true, obj.GetSprintDepletion(), obj.GetAllowSprint());

        if (playerController.animator != null)
        {
            playerController.animator.SetBool("isIdle", false);
            playerController.animator.SetBool("isWalking", false);
            playerController.animator.SetBool("isGrabbing", true);
        }
        

        if (showDebugLogs)
        {
            Debug.Log($"Grabbed " + obj.gameObject.name + ". Occupied status: " + IsOccupied());
        }
    }

    public void OnReleaseObject(MoveableObject obj)
    {
        if (playerController.animator != null)
            playerController.animator.SetBool("isGrabbing", false);
        playerController.Speed = normalMoveSpeed;
        playerController.SprintSpeed = normalSprintSpeed;
        playerController.MovingObject(false, 1f);

        if (playerController.animator != null)
        {
            playerController.animator.SetBool("isGrabbing", false);
            //resetAnimations(); //exit animation state
        }

        movedObjects.Remove(obj);
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

    public bool IsOccupied() => movedObjects.Count > 0;

}
