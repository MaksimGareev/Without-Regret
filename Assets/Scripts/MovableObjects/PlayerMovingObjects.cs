using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerMovingObjects : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("Where moveable objects will snap to (should already be set)")] public Transform grabPoint;
    [Tooltip("Which layers will be ignored when checking for collisions while a MoveableObject is held.")]
    [SerializeField] private LayerMask ignoreCollisionLayer;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private bool isGrabbing;

    [Tooltip("Determines if player can pick up an object")] 
    public bool canPickUp = true;
    [Tooltip("Determines if player can place down an object")] 
    public bool canPlace;

    private PlayerController playerController;
    private PlayerMantling playerMantling;
    private float normalMoveSpeed;
    private float normalSprintSpeed;
    private HashSet<MoveableObject> movedObjects = new HashSet<MoveableObject>();

    private Animator animator;
    private CharacterSwap characterSwap;


    private void Awake()
    {
        canPickUp = true;
        characterSwap = FindFirstObjectByType<CharacterSwap>();

        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }
        playerController = gameObject.GetComponent<PlayerController>();
        playerMantling = gameObject.GetComponent<PlayerMantling>();
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
        canPlace = false; //Prevent player from placing down or picking up during pickup animation
        canPickUp = false;
        animator.SetTrigger("pickup");
        Debug.Log("Picking up!");
        playerController.DisableInput();
        playerMantling.canMantle = false;
        yield return new WaitForSeconds(1.5f);
        playerController.EnableInput();
        playerMantling.canMantle = true;
        canPlace = true; //Allow player to place down objects
        animator.ResetTrigger("pickup"); //Safety pickup trigger reset
    }

    IEnumerator PlaceDown()
    {
        canPickUp = false; //Prevent player from placing down or picking up during Placing animation
        canPlace = false;
        animator.SetTrigger("placing");
        Debug.Log("Placing down!");
        playerController.DisableInput();
        playerMantling.canMantle = false;
        yield return new WaitForSeconds(1.0f);
        playerController.EnableInput();
        playerMantling.canMantle = true;
        ResetAnimations();
        canPickUp = true; //Allow player to pick up objects
        animator.ResetTrigger("placing"); //Safety place trigger reset
    }

    private void ResetAnimations()
    {
        if (showDebugLogs) Debug.Log("Reset animations");
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isGrabbing", false);
        animator.SetBool("isFloating", false);
    }

    void UpdateAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

    // Returns true if any objects are currently held
    public bool IsOccupied() => movedObjects.Count > 0;

    // Predict whether moving the player by 'delta' (world-space) would cause any held object to overlap environment colliders.
    // This uses the held object's world bounds as an approximate test (OverlapBox).
    public bool CanMoveByPosition(Vector3 delta)
    {
        if (!IsOccupied()) return true;

        foreach (var obj in movedObjects)
        {
            if (!obj.CheckMovement) continue;

            Collider col = obj.ObjectCollider;
            if (col == null) continue;

            // compute the target bounds after moving player by delta (held object moves with player as it's parented to grab point)
            Bounds b = col.bounds;
            Vector3 targetCenter = b.center + delta;
            Vector3 extents = b.extents * obj.CollisionCheckSizeFactor; // adjust the size of the box used for checking collisions based on the object's setting
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
                    Debug.Log($"CanMoveByPosition: movement blocked by {hit.name} (held object {obj.name} would overlap)");
                }
                return false;
            }
        }

        return true;
    }

    public bool CanMoveByRotation(Quaternion targetRotation)
    {
        if (!IsOccupied()) return true;

        foreach (var obj in movedObjects)
        {
            if (!obj.CheckRotation) continue;

            Collider col = obj.ObjectCollider;
            if (col == null) continue;

            Vector3 simulatedPosition = SimulateWorldPosition(transform, targetRotation, grabPoint, obj.transform.localPosition);
            Quaternion simulatedRotation = SimulateWorldRotation(targetRotation, grabPoint, obj.transform.localRotation);

            Collider[] hits = new Collider[8];
            Physics.OverlapSphereNonAlloc(obj.transform.position, 5f, hits, ~ignoreCollisionLayer, QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit == col) continue;
                if (hit.transform == null) continue;
                // ignore any colliders that belong to the player (so player's own collider won't block)
                if (hit.transform.IsChildOf(transform)) continue;
                // ignore the held object hierarchy
                if (hit.transform.IsChildOf(obj.transform)) continue;

                if (Physics.ComputePenetration(
                    col, simulatedPosition, simulatedRotation,
                    hit, hit.transform.position, hit.transform.rotation,
                    out Vector3 _, out float _))
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"CanMoveByRotation: movement blocked by {hit.name} (held object {obj.name} would overlap)");
                    }
                    return false;
                }
            }
        }

        return true;

        //foreach (var obj in movedObjects)
        //{
        //    Collider col = obj.ObjectCollider;
        //    if (col == null) continue;

        //    // compute the target bounds after moving player by delta (held object moves with player as it's parented to grab point)
        //    Bounds b = col.bounds;
        //    Vector3 targetCenter = b.center;
        //    Vector3 extents = b.extents * obj.CollisionCheckSizeFactor; // adjust the size of the box used for checking collisions based on the object's setting
        //    Quaternion rotation = targetRotation;

        //    // Query for overlapping colliders at the target location
        //    Collider[] hits = Physics.OverlapBox(targetCenter, extents, rotation, ~ignoreCollisionLayer, QueryTriggerInteraction.Ignore);
        //    foreach (var hit in hits)
        //    {
        //        if (hit == col) continue; // ignore self
        //        // ignore any colliders that belong to the player (so player's own collider won't block)
        //        if (hit.transform.IsChildOf(transform)) continue;
        //        // ignore the held object hierarchy
        //        if (hit.transform.IsChildOf(obj.transform)) continue;

        //        // Any other hit means we'd clip into something
        //        if (showDebugLogs)
        //        {
        //            Debug.Log($"CanMoveByRotation: movement blocked by {hit.name} (held object {obj.name} would overlap)");
        //        }
        //        return false;
        //    }
        //}

        //return true;
    }

    // Helpers for calculating collision clipping
    Vector3 SimulateWorldPosition(Transform player, Quaternion playerTargetRotation, Transform grabPoint, Vector3 objectLocalPos)
    {
        Vector3 grabWorldPos = playerTargetRotation * grabPoint.localPosition + player.position;

        Quaternion grabWorldRot = playerTargetRotation * grabPoint.localRotation;

        return grabWorldPos + grabWorldRot * objectLocalPos;
    }

    Quaternion SimulateWorldRotation(Quaternion playerTargetRotation, Transform grabPoint, Quaternion objectLocalRot)
    {
        Quaternion grabWorldRot = playerTargetRotation * grabPoint.localRotation;

        return grabWorldRot * objectLocalRot;
    }

}
