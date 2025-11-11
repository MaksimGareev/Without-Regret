using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyDistracted : MonoBehaviour
{
    [Header("Distraction Settings")]
    [SerializeField] private float lingerDistance = 1.5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lingerTime = 2f;

    private PatrollingEnemy enemyMovement;
    private NavMeshAgent enemyNavMeshAgent;

    private bool isDistracted = false;
    private Vector3 distractionPoint;
    private float distractionTimer;
    private float lingerTimer;
    private float originalSpeed;

    private void Awake()
    {
        enemyMovement = GetComponent<PatrollingEnemy>();
        enemyNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isDistracted)
        {
            distractionTimer -= Time.deltaTime;

            if (enemyNavMeshAgent.remainingDistance <= lingerDistance)
            {
                lingerTimer += Time.deltaTime;

                if (lingerTimer >= lingerTime || distractionTimer <= 0f)
                {
                    EndDistraction();
                }
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
        lingerTimer = 0f;

        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        if (enemyNavMeshAgent != null)
        {
            originalSpeed = enemyNavMeshAgent.speed;
            enemyNavMeshAgent.enabled = true;
            enemyNavMeshAgent.isStopped = false;
            enemyNavMeshAgent.speed = moveSpeed;
            enemyNavMeshAgent.SetDestination(distractionPoint);
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
            enemyNavMeshAgent.speed = originalSpeed;
            enemyNavMeshAgent.ResetPath();
        }
    }
}
