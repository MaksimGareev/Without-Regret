using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class EnemyDistracted : MonoBehaviour
{
    [Header("Distraction Settings")]
    [SerializeField] private float lingerDistance = 1.5f;
    [SerializeField] private float moveSpeed = 25f;

    private Enemy enemyMovement;
    private NavMeshAgent enemyNavMeshAgent;
    private Rigidbody rb;
    private bool isDistracted = false;
    private Vector3 distractionPoint;
    private float distractionTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyMovement = GetComponent<Enemy>();
        enemyNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isDistracted)
        {
            distractionTimer -= Time.deltaTime;

            float distance = Vector3.Distance(transform.position, distractionPoint);
            if (distance > lingerDistance)
            {
                Vector3 direction = (distractionPoint - transform.position).normalized;
                rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
            }

            if (distractionTimer <= 0f)
            {
                EndDistraction();
            }
        }
    }

    public void BeginDistraction(Vector3 distractionPos, float duration)
    {
        if (isDistracted)
        {
            return;
        }

        isDistracted = true;
        distractionPoint = distractionPos;
        distractionTimer = duration;

        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        if (enemyNavMeshAgent != null)
        {
            enemyNavMeshAgent.enabled = false;
        }
    }

    private void EndDistraction()
    {
        isDistracted = false;
        if (enemyMovement != null)
        {
            enemyMovement.enabled = true;
        }
        if (enemyNavMeshAgent != null)
        {
            enemyNavMeshAgent.enabled = true;
        }
    }
}
