using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingEnemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range; // Area around the center point
    public Transform centrePoint; // Center Point of designated area
    public Animator animator;

    public float waitTime = 2f;
    [SerializeField] private bool isWaiting = false;

    // Player morality effects
    public float baseSpeed = 3.5f;
    public float minSpeed = 1.5f;
    public float maxSpeed = 6.5f;
    public float moralitySpeedMultiplier = 0.15f;
    public DialogueManager playerMorality;

    public bool chasing;

    // Damaging player
    public float damageTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PickNewDestination();
    }

    void Update()
    {
        UpdateSpeedFromMorality();//this check will keep happening every frame. perhaps we could change it to happen only once upon seeing the player? It might not affect to much in terms of performance now, but could be an issue if we have a lot of enemies in a level.

        // If close to destination and not already waiting, start waiting
        if (!isWaiting && !agent.pathPending && !chasing && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitBeforeNextMove());
        }

        float currentSpeed = agent.velocity.magnitude; //gets the speed of the enemy to handle animation states

        if (currentSpeed > 0.1f)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isIdle", false);
        }
        else
        {
            animator.SetBool("isIdle", true);
            animator.SetBool("isWalking", false);
        }
    }

    public void UpdateSpeedFromMorality()
    {
        if (playerMorality == null) return;

        int morality = playerMorality.playerMorality;

        float speedOffset = -morality * moralitySpeedMultiplier;

        float newSpeed = baseSpeed + speedOffset;

        agent.speed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
    }

    // Timer for NPC to pick next random point
    IEnumerator WaitBeforeNextMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime); // Wait at the point
        PickNewDestination(); // Pick next destination
        isWaiting = false;
    }

    void PickNewDestination()
    {
        if (centrePoint == null)
        {
            return;
        }
        if (RandomPoint(centrePoint.position, range, out Vector3 point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); // Show next point
            agent.SetDestination(point); // Move NPC to point
        }
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; // Picking random point within range
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;

            return true;
        }

        result = Vector3.zero;
        // Debug.Log("search for point failed");
        return false;

    }

    public void OnTriggerEnter(Collider other)
    {/*
        if (other.CompareTag("Player"))
        {
           // damageTimer = 2f;
           // damageTimer -= Time.deltaTime;

            if (TimerRingUI.Instance != null && damageTimer == 0)
            {
                TimerRingUI.Instance.SubtractRingSection(1);
            }
        }*/
    }
}
