using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyDistracted : MonoBehaviour
{
    [Header("Distraction Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lingerTime = 2f;
    [SerializeField] private float stoppingRadius = 1f;


    private PatrollingEnemy enemyMovement;
    private NavMeshAgent enemyNavMeshAgent;
    private GameObject distraction;

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
        if (!isDistracted || distraction == null)
        {
            return;
        }

        distractionTimer -= Time.deltaTime;

        if (enemyNavMeshAgent != null && !enemyNavMeshAgent.pathPending)
        {
            float distance = Vector3.Distance(transform.position, distractionPoint);

            if (distance <= stoppingRadius && HasLineOfSight(distraction))
            {
                enemyNavMeshAgent.isStopped = true;
                enemyNavMeshAgent.velocity = Vector3.zero;
                lingerTimer += Time.deltaTime;
            }
        }

        if (lingerTimer >= lingerTime || distractionTimer <= 0f)
        {
            EndDistraction();
        }
    }

    private bool HasLineOfSight(GameObject distraction)
    {
        Vector3 direction = distractionPoint - transform.position;
        Vector3 start = transform.position + Vector3.up * 1.5f + Vector3.forward * 0.5f;

        if (Physics.Raycast(start, direction.normalized, out RaycastHit hit, direction.magnitude))
        {
            if (hit.collider.gameObject == distraction)
            {
                return true;
            }
        }

        return false;
    }

    public void BeginDistraction(Vector3 distractionPos, float duration, GameObject newDistraction)
    {
        if (isDistracted)
        {
            return;
        }

        isDistracted = true;
        distractionPoint = distractionPos;
        distractionTimer = duration;
        lingerTimer = 0f;
        distraction = newDistraction;

        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        if (enemyNavMeshAgent != null)
        {
            originalSpeed = enemyNavMeshAgent.speed;
            enemyNavMeshAgent.isStopped = false;
            enemyNavMeshAgent.speed = moveSpeed;
            enemyNavMeshAgent.SetDestination(distractionPoint);
        }
    }

    private void EndDistraction()
    {
        isDistracted = false;

        distraction = null;

        if (enemyMovement != null)
        {
            enemyMovement.enabled = true;
        }

        if (enemyNavMeshAgent != null)
        {
            enemyNavMeshAgent.speed = originalSpeed;
            enemyNavMeshAgent.isStopped = false;
            enemyNavMeshAgent.ResetPath();
        }
    }
}
