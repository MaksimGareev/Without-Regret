using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFieldOfView : MonoBehaviour
{
    public float radius; // field of view radius around player
    public float aggroRadius; // bigger radius that the enemy uses when chasing an entity
    [Range(0, 360)]
    public float angle; // viewing angle of enemy
    private float m_Distance;

    public GameObject playerRef; // object enemy is looking for
    private NavMeshAgent m_Agent; // NavMesh variable for enemy

    public LayerMask targetMask; // layer of what the enemy targets
    public LayerMask obstructionMask; // layer of objects that block the enmey's view

    public bool canSeePlayer; // if the player is in the enemy's field of view
    public float chaseDuration = 1; 
    public float maxChaseDuration = 1;

    public float detectionRadius = 4f; //detects player/NPC if they get too close, regardless of whether they are in the FOV or not
    

    // Start is called before the first frame update
    void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());
        m_Agent = GetComponent<NavMeshAgent>();
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
                    m_Agent.destination = target.position; // if seen move towards the player
                    angle = 230; // enemy FOV widens while chasing the player
                    radius = aggroRadius;
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
                    angle = 90;
                    radius = 5;
                    Debug.Log("Player lost");
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
            angle = 90;
            radius = 5;
            Debug.Log("Player lost");

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
}