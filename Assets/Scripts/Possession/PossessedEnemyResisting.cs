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
    [SerializeField] private float struggleStrength = 0.5f; // The strength of the enemy's resistance
    [SerializeField, Range(0f, 1f)] private float struggleFrequency = 0.5f; // How often the enemy tries to resist the possession

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

        //struggleTimer += Time.fixedDeltaTime;

        //if (struggleTimer >= struggleFrequency)
        //{
        //struggleTimer = 0f;

        //struggleDirection = playerInput + (Random.insideUnitSphere * struggleStrength);
        //struggleDirection.y = 0f;
        //}

        Vector3 finalMoveDirection = gameObject.transform.position + playerInput.normalized * moveSpeed * Time.deltaTime * -1;
        finalMoveDirection.y = 0;

        //rb.MovePosition(rb.position + finalMoveDirection * Time.fixedDeltaTime);

        Agent.destination = finalMoveDirection;

        Debug.DrawRay(transform.position, moveDirection, Color.white, 0.1f);
        Debug.DrawRay(transform.position, struggleDirection, Color.red, 0.1f);
        //Debug.DrawRay(transform.position, finalMoveDirection, Color.green, 0.1f);
    }

    public void BeginPossession()
    {
        isPossessed = true;
        //struggleTimer = 0f;
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
