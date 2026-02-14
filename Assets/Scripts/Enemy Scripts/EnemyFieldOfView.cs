using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyFieldOfView : MonoBehaviour
{
    // FOV settings
    public float minRadius = 4f;
    public float maxRadius = 8f;
    public float minAngle = 50f;
    public float maxAngle = 130f;
    public float moralityEffect = 0.5f;

    private float baseRadius;
    private float baseAngle;

    // Detection Settings
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    // player reference
    public Transform playerRef;

    //animator reference
    public Animator animator;

    // Smoothing
    public bool smoothFOV = true;
    public float fovSmoothSpeed = 2f;

    public DialogueManager dialogueManager;

    public float radius; // field of view radius around player
    public float aggroRadius; // bigger radius that the enemy uses when chasing an entity
    [Range(0, 360)]
    public float angle; // viewing angle of enemy
    private float m_Distance;

    public GameObject playerObj; // object enemy is looking for
    private NavMeshAgent m_Agent; // NavMesh variable for enemy

    public LayerMask targetMask; // layer of what the enemy targets
    public LayerMask obstructionMask; // layer of objects that block the enmey's view

    public bool canSeePlayer; // if the player is in the enemy's field of view
    public float chaseDuration = 1; 
    public float maxChaseDuration = 1;

    public float detectionRadius = 4f; //detects player/NPC if they get too close, regardless of whether they are in the FOV or not

    //attack handlers
    public float attackRadius = 3f;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;
    public bool isAttacking;

    private bool isStunned = false;
    private float stunDuration = 2f;
    private float timeSinceStunned = 0f;

    [SerializeField] private PatrollingEnemy normalMovement;

    private void Start()
    {
        StartCoroutine(FOVRoutine());
    }

    void Update()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerRef == null && playerObj != null)
        {
            playerRef = playerObj.transform;
        }

        if (isStunned)
        {
            if(timeSinceStunned >= stunDuration)
            {
                isStunned = false;
                timeSinceStunned = 0;
            }
            timeSinceStunned += Time.deltaTime;
        }
        
        m_Agent = GetComponent<NavMeshAgent>();

        UpdateFOVBasedOnMorality();
        ApplyFOV();
        DetectPlayer();
        
    }

    private void UpdateFOVBasedOnMorality()
    {
        if (dialogueManager == null) return;

        // Get plalyers current morality
        int playerMorality = dialogueManager.playerMorality;
        float normalizedMorality = Mathf.Clamp(playerMorality / 10f, -1f, 1f) * moralityEffect;
        float t = (normalizedMorality + 1f) / 2f;

        baseRadius = Mathf.Lerp(maxRadius, minRadius, (normalizedMorality + 1f) / 2f);
        baseAngle = Mathf.Lerp(maxAngle, minAngle, (normalizedMorality + 1f) / 2f);

        /*if (!canSeePlayer)
        {

            radius = baseRadius;
            angle = baseAngle;

            //radius = Mathf.Lerp(radius, targetRadius, Time.deltaTime * fovSmoothSpeed);
            //angle = Mathf.Lerp(angle, targetAngle, Time.deltaTime * fovSmoothSpeed);
        }
        else
        {
           // radius = targetRadius;
            //angle = targetAngle;
        }*/

        Debug.Log($"Morality : {playerMorality}, Radius: {radius}, Angle: {angle}");
    }

    private void DetectPlayer()
    {
        if (playerRef == null) return;

        Vector3 directionToPlayer = (playerRef.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, playerRef.position) <= radius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer <= angle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToPlayer, out RaycastHit hit, radius, obstacleMask))
                {
                    canSeePlayer = true;
                    return;
                }
            }
        }

        canSeePlayer = false;
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(.02f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    // Function for enemy's field of view
    private void FieldOfViewCheck()
    {
        if (AttackRangeCheck())
            return;
        if (CloseDetectionCheck())
            return;
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0) // checking if the player is in the given range of the enemy
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                m_Distance = Vector3.Distance(m_Agent.transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask)) // raycast vision of the enemy
                {
                    chaseDuration = 1;
                    canSeePlayer = true;
                    normalMovement.chasing = true;
                    if (!isStunned)
                    {
                        m_Agent.destination = target.position; // if seen move towards the player
                    }
                    //angle = Mathf.Max(baseAngle,230); // enemy FOV widens while chasing the player
                    //radius = aggroRadius;
                    currentState = FOVState.Chasing;
                    return;
                    //Debug.Log("Player detected");
                }
            }
            if (canSeePlayer) //if player breaks line of sight enemy can still chase the player but chase timer will go down
            {
                m_Agent.destination = target.position;
                chaseDuration -= Time.deltaTime;
                if (chaseDuration <= 0) //if player breaks line of sight/outruns
                {
                    canSeePlayer = false;
                    currentState = FOVState.Idle;
                    normalMovement.chasing = false;
                    //angle = 90;
                    //radius = 5;
                    //Debug.Log("Player lost");
                }
            }
            //canSeePlayer = false;
            //angle = 90;
            //if (!canSeePlayer)
            //{
            //    canSeePlayer = false;
            //    angle = 90; //enemy FOV decreases back to normal when player is lost
            //    Debug.Log("Player lost");
            //}
        }
        else
        {
            canSeePlayer = false;
            currentState = FOVState.Idle;
            //angle = 90;
            //radius = 5;
            //Debug.Log("Player lost");
           
        }
    }

    private void ApplyFOV()
    {
        float targetRadius = radius;
        float targetAngle = angle;

        switch (currentState)
        {
            case FOVState.Idle:
                radius = baseRadius;
                angle = baseAngle;
                break;

            case FOVState.Alerted:
                radius = baseRadius * 1.2f;
                angle = Mathf.Max(baseAngle, 150f);
                break;

            case FOVState.Chasing:
                radius = aggroRadius;
                angle = Mathf.Max(baseAngle, 230f);
                break;
        }

        if (smoothFOV)
        {
            radius = Mathf.Lerp(radius, targetRadius, Time.deltaTime * fovSmoothSpeed);
            angle = Mathf.Lerp(angle, targetAngle, Time.deltaTime * fovSmoothSpeed);
        }
        else
        {
            radius = targetRadius;
            angle = targetAngle;
        }
    }

    // Function for killing the player
    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            Debug.Log("Player is killed");
        }

        if (other.gameObject.CompareTag("protectedNPC"))
        {
            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(3);
            }
            //if enemy attacks NPC, trigger game over screen
        }

        if (other.gameObject.CompareTag("Throwable"))
        {
            if (other.gameObject.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > 0.001f)
            {
                other.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                m_Agent.Stop();
                normalMovement.animator.SetBool("isIdle", true);
                isStunned = true;
                Debug.Log("Hit with throwable");
            }
        }
    }

    // check to see if the player is alive
    void OnTriggerExit(Collider other)
    {
        if (other.name == "Player")
        {
            //canSeePlayer = false;
            Debug.Log("Player is alive");
        }
    }

    private bool CloseDetectionCheck() //handles detection of target layer mask, when entity gets to close to an enemy, the enemy will aggro onto that enemy, regardless of it being in FOV or not
    {
        Collider[] closeTargets = Physics.OverlapSphere(transform.position, detectionRadius, targetMask);

        if (closeTargets.Length > 0)
        {
            Transform target = closeTargets[0].transform;
            chaseDuration = 1;
            canSeePlayer = true;
            m_Agent.destination = target.position;
            angle = 230;
            radius = aggroRadius;

            return true;
        }
        return false;
    }

    private bool AttackRangeCheck() //handles detection of target layer mask, when entity gets to close to an enemy, the enemy will aggro onto that enemy, regardless of it being in FOV or not
    {
        Collider[] closeTargets = Physics.OverlapSphere(transform.position, attackRadius, targetMask);

        if (closeTargets.Length > 0)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                StartCoroutine(attackAnimation());
            }
            return true;
        }
        return false;
    }

    private void EnemyAnimations()
    {
        bool isMoving = m_Agent.velocity.sqrMagnitude > 0.1f && m_Agent.remainingDistance > m_Agent.stoppingDistance;
        if (!isAttacking)
        {
            if (isMoving)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isIdle", false);
            }
            else if (!isMoving)
            {
                animator.SetBool("isIdle", true);
                animator.SetBool("isWalking", false);
            }
        }
    }

    IEnumerator attackAnimation()
    {
        if (!isAttacking)
        {
            resetanimations();
        }
        isAttacking = true;
        animator.SetTrigger("Attack");
        Debug.Log("Attacked");
        yield return new WaitForSeconds(2f);
        isAttacking = false;
    }

    private void resetanimations()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);

    }

    private enum FOVState
    {
        Idle,
        Alerted,
        Chasing
    }

    private FOVState currentState = FOVState.Idle;
}