using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chime : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private bool facePlayer = true;
    [SerializeField] private float lookSmooth = 8f;
    [SerializeField] private float bobHeight = .5f;
    [SerializeField] private float bobSpeed = 2f;
    [Tooltip("Smoothing speed for following the player")]
    [SerializeField] private float followSmooth = 10f;

    [Header("Orbiting")]
    [Tooltip("If false, will use leading behavior instead of orbiting")]
    [SerializeField] private bool orbitPlayer = false;
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 2f;

    [Header("Leading")]
    [Tooltip("Distance ahead of the player when they are moving")]
    [SerializeField] private float leadDistance = 2f;
    [Tooltip("Distance in front of the player when they are stationary")]
    [SerializeField] private float leadStationaryDistance = 1.5f;
    [Tooltip("Base vertical offset above player's position")]
    [SerializeField] private float leadVerticalOffset = 1f;
    [Tooltip("Minimum horizontal speed to consider the player as 'moving'")]
    [SerializeField] private float moveThreshold = 0.1f;

    private float orbitAngle;
    //private Transform OrbitPivot;
    //private Transform BobObject;

    public static bool isInDialogue = false;

    [Header("Animator")]
    public Animator animator;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos;

        if (!isInDialogue)
        {
            if (orbitPlayer)
            {
                // Orbit angle increases steadily
                orbitAngle += orbitSpeed * Time.deltaTime;
                if (orbitAngle > Mathf.PI * 2f) orbitAngle -= Mathf.PI * 2f;

                // Calculate orbit position relative to player
                Vector3 offset = new Vector3(Mathf.Cos(orbitAngle) * orbitRadius, Mathf.Sin(Time.time * bobSpeed) * bobHeight + 1f, Mathf.Sin(orbitAngle) * orbitRadius);

                // Smoothly rotate toward player
                targetPos = player.position + offset;
            }
            else
            {
                // Leading behavior:
                // Try to use the player's CharacterController velocity first (PlayerController uses a CharacterController),
                // otherwise try Rigidbody velocity, then otherwise use player's forward.
                Vector3 playerVelocity = Vector3.zero;

                if (player.TryGetComponent<PlayerController>(out var pc) && pc.Controller != null)
                {
                    playerVelocity = pc.Controller.velocity;
                }
                else
                {
                    if (player.TryGetComponent<Rigidbody>(out var rb))
                        playerVelocity = rb.linearVelocity;
                }

                // Vertical bob
                float bobY = Mathf.Sin(Time.time * bobSpeed) * bobHeight + leadVerticalOffset;
                Vector3 bob = new Vector3(0f, bobY, 0f);

                // If player is moving, place Chime ahead in the movement direction
                Vector3 horizontalVel = playerVelocity;
                horizontalVel.y = 0f;

                Vector3 lookDir;
                if (horizontalVel.sqrMagnitude > moveThreshold * moveThreshold)
                {
                    Vector3 moveDir = horizontalVel.normalized;
                    lookDir = moveDir;
                    targetPos = player.position + moveDir * leadDistance + bob;
                }
                else
                {
                    // Player standing / not moving: place Chime a little in front of the player
                    lookDir = player.forward;
                    targetPos = player.position + player.forward * leadStationaryDistance + new Vector3(0f, leadVerticalOffset, 0f) + bob;
                }

                if (!facePlayer)
                {
                    // Look in the direction of movement (or player's forward if stationary)
                    if (lookDir.sqrMagnitude > 0.001f)
                    {
                        Quaternion lookRot = Quaternion.LookRotation(lookDir, Vector3.up);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookSmooth);
                    }
                }
            }
        }
        else
        {
            // Dialogue Mode
            Vector3 dialogueOffset = player.right * 1.2f + new Vector3(0f, 1f, 0f);
            Vector3 bob = new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0f);

            targetPos = player.position + player.forward * 1.5f + dialogueOffset + bob;
        }

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmooth);

        // smoothly rotate toward player
        if (facePlayer)
        {
            // look at players horizontal position only
            Vector3 lookPoint = player.position;
            lookPoint.y = transform.position.y; // keep chime level

            Vector3 dir = lookPoint - transform.position;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookSmooth);
            }
        }
    }

    //Chime's Animation functions

    public void SetIdleAnimation()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);
    }
    public void setSpecialIdleAnimation()
    {
        animator.SetBool("isInSpecialIdle", true);
        animator.SetTrigger("specialIdle");
    }

    public void SetWalkingAnimation()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);
    }

    public void setFloatingAnimation()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isFloating", true);
    }


    public void ResetChimeAnimations()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isFloating", false);
    }
}
