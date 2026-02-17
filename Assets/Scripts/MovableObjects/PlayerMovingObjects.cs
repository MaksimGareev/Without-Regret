using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovingObjects : MonoBehaviour
{
    private Animator animator;
    [Header("General Settings")]
    [Tooltip("Where moveable objects will snap to (should already be set)")] public Transform grabPoint;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    //[Header("Animator Settings")]
    private bool isGrabbing;

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
        if (IsOccupied() || !movedObjects.Add(obj)) return;

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
            GrabbingAnimationHandler();
            if (showDebugLogs) Debug.Log("Grabbed");
        }
        
        normalMoveSpeed = playerController.Speed;
        normalSprintSpeed = playerController.SprintSpeed;
        playerController.Speed = normalMoveSpeed / obj.GetMoveSlowdown();
        playerController.SprintSpeed = normalSprintSpeed / obj.GetSprintSlowdown();
        playerController.MovingObject(true, obj.GetSprintDepletion(), obj.GetSprintTimerDecay(), obj.GetAllowSprint());

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
        StartCoroutine(placeDown());
        //if (playerController.animator != null)
        //    playerController.animator.SetBool("isGrabbing", false);
        playerController.Speed = normalMoveSpeed;
        playerController.SprintSpeed = normalSprintSpeed;
        playerController.MovingObject(false);

        //if (playerController.animator != null)
        //{
        //    playerController.animator.SetBool("isGrabbing", false);
        //    //resetAnimations(); //exit animation state
        //}

        movedObjects.Remove(obj);
    }

    public void GrabbingAnimationHandler()
    {
        if (!isGrabbing)
        {
            resetAnimations();
        }
        isGrabbing = true;
        StartCoroutine(pickup());
        animator.SetBool("isGrabbing", true);
        if (showDebugLogs) Debug.Log("Grabbing");
    }

    IEnumerator pickup()
    {
        animator.SetTrigger("pickup");
        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator placeDown()
    {
        animator.SetTrigger("placing");
        yield return new WaitForSeconds(1.5f);
        resetAnimations();
    }



    private void resetAnimations()
    {
        if (showDebugLogs) Debug.Log("Reset animations");
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isGrabbing", false);
        animator.SetBool("isFloating", false);
    }

    public bool IsOccupied() => movedObjects.Count > 0;
}
