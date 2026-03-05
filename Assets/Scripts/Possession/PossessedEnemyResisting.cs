using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class PossessedEnemyResisting : MonoBehaviour
{
    [Header("Possession Settings")]
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private float moveSpeed = 20f;

    private bool isPossessed = false;
    private Vector3 playerInput;
    private float struggleTimer;
    private Camera PlayerCamera;

    private Vector3 struggleDirection;


    private void Awake()
    {
        if (PlayerCamera == null)
        {
            PlayerCamera = Camera.main;
        }
    }
    private void FixedUpdate()
    {
        if (!isPossessed)
        {
            return;
        }
        Vector3 move = Vector3.zero;
        if (PlayerCamera != null)
        {
            Vector3 camForward = PlayerCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = PlayerCamera.transform.right;
            camRight.y = 0f;
            camRight.Normalize();
            move = camForward * playerInput.y + camRight * playerInput.x;

        }
        Vector3 finalMoveDirection = gameObject.transform.position + move.normalized * moveSpeed * Time.deltaTime;

        Agent.destination = finalMoveDirection;


    }

    public void BeginPossession()
    {
        isPossessed = true;
    }

    public void UpdatePossession(Vector3 input)
    {
        playerInput = input;
    }

    public void EndPossession()
    {
        isPossessed = false;
        playerInput = Vector3.zero;
    }
}
