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

    private Vector3 struggleDirection;


    private void FixedUpdate()
    {
        if (!isPossessed)
        {
            return;
        }

        Vector3 moveDirection = playerInput.normalized * moveSpeed;

        Vector3 finalMoveDirection = gameObject.transform.position + playerInput.normalized * moveSpeed * Time.deltaTime * -1;
        finalMoveDirection.y = 0;

        Agent.destination = finalMoveDirection;

        Debug.DrawRay(transform.position, moveDirection, Color.white, 0.1f);
        Debug.DrawRay(transform.position, struggleDirection, Color.red, 0.1f);
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
