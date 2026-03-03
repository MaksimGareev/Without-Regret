using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chime : MonoBehaviour
{
    public Transform player;

    public float OrbitRadius = 2f;
    public float OrbitSpeed = 2f;

    public float BobHeight = .5f;
    public float BobSpeed = 2f;

    public bool facePlayer = true;
    public float lookSmooth = 8f;

    private float OrbitAngle;

    private Transform OrbitPivot;
    private Transform BobObject;
    public Transform model;

    public static bool isInDialogue = false;

    [Header("Animator")]
    public Animator animator;


    // Update is called once per frame
    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos;

        if (!isInDialogue)
        {
            // Orbit angle increases steadily
            OrbitAngle += OrbitSpeed * Time.deltaTime;
            if (OrbitAngle > Mathf.PI * 2f) OrbitAngle -= Mathf.PI * 2f;

            // Calculate orbit position relative to player
            Vector3 offset = new Vector3(Mathf.Cos(OrbitAngle) * OrbitRadius, Mathf.Sin(Time.time * BobSpeed) * BobHeight + 1f, Mathf.Sin(OrbitAngle) * OrbitRadius);

            // Smoothly rotate toward player
            targetPos = player.position + offset;
        }
        else
        {
            // Dialogue Mode
            Vector3 dialogueOffset = player.right * 1.2f + new Vector3(0f, 1f, 0f);

            //new Vector3(0f, Mathf.Sin(Time.time * BobSpeed) * BobHeight + 1f, OrbitRadius * 0.5f);
            Vector3 bob = new Vector3(0f, Mathf.Sin(Time.time * BobSpeed) * BobHeight, 0f);

            targetPos = player.position + player.forward * 1.5f + dialogueOffset + bob;
        }

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);

        // smoothly rotate toward player
        if (facePlayer && model != null)
        {
            // look at players horizontal potiion only
            Vector3 lookPoint = player.position;
            lookPoint.y = model.position.y; // keep chime level

            Vector3 dir = lookPoint - model.position;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                model.rotation = Quaternion.Slerp(model.rotation, lookRot, Time.deltaTime * lookSmooth);
            }
        }
    }

    //Chimes Animation functions

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
