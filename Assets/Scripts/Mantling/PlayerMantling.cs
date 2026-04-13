using System;
using System.Collections;
using UnityEngine;

public class PlayerMantling : MonoBehaviour
{
    [Header("Mantling Settings")]
    //[SerializeField] private float mantleRange = 2f;
    [SerializeField] private float mantleSpeed = 6f;
    [SerializeField] private float mantleHeight = 3f;
    [SerializeField] public bool canMantle = true;

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    [Header("Animator")]
    public Animator animator;
    private CharacterSwap characterSwap;

    private CharacterController controller;
    private PlayerController playerController;
    private PlayerFloating playerFloating;
    private PlayerMovingObjects playerMovingObjects;
    private PlayerPossessing playerPossessing;
    private PlayerThrowing playerThrowing;

    public bool isMantling = false;
    private Vector3 mantleStartPos;
    private Vector3 mantleEndPos;
    private float mantleProgress = 0f;
    private Action mantleCompleteCallback; // Tells the mantleable object that the mantle ended

    private void Awake()
    {
        characterSwap = FindObjectOfType<CharacterSwap>();

        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }

        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        playerFloating = GetComponent<PlayerFloating>();
        playerMovingObjects = GetComponent<PlayerMovingObjects>();
        playerPossessing = GetComponent<PlayerPossessing>();
        playerThrowing = GetComponent<PlayerThrowing>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMantling)
        {
            PerformMantle();
            return;
        }   
    }

    public void StartMantle(MantleableObject point, Action completionCallback = null)
    {
        if (canMantle) //mantle check to prevent mantling while performing certain actions
        {
            //Checks object height to determine if mantleable object is too tall/is mantleable from current position
            float heightDifference = point.GetMantlePosition().y - transform.position.y; //height difference is difference between mantle end point and current player transform point
            if (heightDifference > mantleHeight) //if height difference is greater than mantle height, cant mantle (Set mantle height higher to mantle taller objects)
            {
                if (showDebugLogs)
                    Debug.Log("Cant Mantle, object is too tall!");

                return;
            }

            isMantling = true;
            if (animator)
                animator.SetBool("isMantling", true);
            mantleStartPos = transform.position;
            mantleEndPos = point.GetMantlePosition();
            mantleProgress = 0f;
            mantleCompleteCallback = completionCallback;

            if (playerController != null)
            {
                playerController.enabled = false;
            }

            if (playerFloating != null)
            {
                playerFloating.enabled = false;
            }

            if (playerMovingObjects != null)
            {
                playerMovingObjects.enabled = false;
            }

            if (playerPossessing != null)
            {
                playerPossessing.enabled = false;
            }

            if (playerThrowing != null)
            {
                playerThrowing.enabled = false;
            }

            if (controller != null)
            {
                controller.enabled = false;
            }
        }
    }

    private void PerformMantle()
    {
        mantleProgress += Time.deltaTime * mantleSpeed;
        if (mantleProgress < 1)
        {
            transform.position = Vector3.Lerp(mantleStartPos, new Vector3(mantleStartPos.x, mantleEndPos.y, mantleStartPos.z), mantleProgress);
        }
        else if (mantleProgress >= 1)
        {
            transform.position = Vector3.Lerp(new Vector3(mantleStartPos.x, mantleEndPos.y, mantleStartPos.z), mantleEndPos, mantleProgress-1);
        }
        if (mantleProgress >= 2f)
        {
            EndMantle();
        }
    }

    private void EndMantle()
    {
        if (showDebugLogs)
        {
            Debug.Log("Mantle complete!");
        }

        isMantling = false;

        mantleCompleteCallback?.Invoke();

        if (animator)
            StartCoroutine(finishedMantling());


        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerFloating != null)
        {
            playerFloating.enabled = true;
        }

        if (playerMovingObjects != null)
        {
            playerMovingObjects.enabled = true;
        }

        if (playerPossessing != null)
        {
            playerPossessing.enabled = true;
        }

        if (playerThrowing != null)
        {
            playerThrowing.enabled = true;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private IEnumerator finishedMantling()
    {
        animator.SetBool("finishedMantling", true);
        yield return new WaitForSeconds(0.3f);
        animator.SetBool("isMantling", false);
        animator.SetBool("finishedMantling", false);
    }

    void UpdateAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

}
