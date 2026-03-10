using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovingObjects : MonoBehaviour
{
    private Animator animator;
    [Header("General Settings")]
    [Tooltip("Where moveable objects will snap to (should already be set)")] public Transform grabPoint;
    [Tooltip("Which layers will be ignored when checking for collisions while a MoveableObject is held.")]
    [SerializeField] private LayerMask ignoreCollisionLayer;
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

        if (playerController.Animator != null)
        {
            playerController.Animator.SetBool("isIdle", false);
            playerController.Animator.SetBool("isWalking", false);
            playerController.Animator.SetBool("isGrabbing", true);
        }
        

        if (showDebugLogs)
        {
            Debug.Log($"Grabbed " + obj.gameObject.name + ". Occupied status: " + IsOccupied());
        }
    }

    public void OnReleaseObject(MoveableObject obj)
    {
        StartCoroutine(PlaceDown());
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
            ResetAnimations();
        }
        isGrabbing = true;
        StartCoroutine(Pickup());
        animator.SetBool("isGrabbing", true);
        if (showDebugLogs) Debug.Log("Grabbing");
    }

    IEnumerator Pickup()
    {
        animator.SetTrigger("pickup");
        Debug.Log("Picking up!");
        playerController.DisableInput();
        yield return new WaitForSeconds(1.5f);
        playerController.EnableInput();
    }

    IEnumerator PlaceDown()
    {
        animator.SetTrigger("placing");
        Debug.Log("Placing down!");
        playerController.DisableInput();
        yield return new WaitForSeconds(1.3f);
        playerController.EnableInput();
        ResetAnimations();
    }



    private void ResetAnimations()
    {
        if (showDebugLogs) Debug.Log("Reset animations");
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isGrabbing", false);
        animator.SetBool("isFloating", false);
    }

    // Returns true if any objects are currently held
    public bool IsOccupied() => movedObjects.Count > 0;

    // Predict whether moving the player by 'delta' (world-space) would cause any held object to overlap environment colliders.
    // This uses the held object's world bounds as an approximate test (OverlapBox).
    public bool CanMoveBy(Vector3 delta)
    {
        if (!IsOccupied()) return true;

        foreach (var obj in movedObjects)
        {
            Collider col = obj.ObjectCollider;
            if (col == null) continue;

            // compute the target bounds after moving player by delta (held object moves with player as it's parented to grab point)
            Bounds b = col.bounds;
            Vector3 targetCenter = b.center + delta;
            Vector3 extents = b.extents * obj.ExtentsMultiplier;
            Quaternion rotation = obj.transform.rotation;

            // Query for overlapping colliders at the target location
            Collider[] hits = Physics.OverlapBox(targetCenter, extents, rotation, ~ignoreCollisionLayer, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (hit == col) continue; // ignore self
                // ignore any colliders that belong to the player (so player's own collider won't block)
                if (hit.transform.IsChildOf(transform)) continue;
                // ignore the held object hierarchy
                if (hit.transform.IsChildOf(obj.transform)) continue;

                // Any other hit means we'd clip into something
                if (showDebugLogs)
                {
                    Debug.Log($"CanMoveBy: movement blocked by {hit.name} (held object {obj.name} would overlap)");
                }
                return false;
            }
        }

        return true;
    }
}
