using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public class ChasingEnemy : MonoBehaviour
{
    // Movement
    public NavMeshAgent agent;
    public Transform[] targets;
    private int currentIndex = 0;
    //private Vector3 StoppingDistance;
    public float PursuitTimer;
    public bool Pursuiting = true;

    // cleaver pickup
    public GameObject CleaverTrig;
    public GameObject CleaverProp;

    // Camera
    public Camera cam;
    public Vector3 offSet;
    public float smoothSpeed = 5f;

    // Check if enemy reached NPC
    public bool ReachedNPC = false;

    // Check if the enemy is possessed or distracted
    public bool Possessed = false;
    public bool Distracted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //StoppingDistance = new Vector3(.7f, .7f, .7f);
        if (targets.Length > 0)
        {
            agent.SetDestination(targets[currentIndex].position);
        }
        else
        {
            Debug.LogWarning("No targets assigned to ChasingEnemy!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // stop enemy when dialogue is active
        if (DialogueManager.DialogueIsActive)
        {
            agent.isStopped = true;
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        if (Possessed || Distracted || targets.Length == 0)
            return;

        // If close enough to the target, switch to next
        if (!agent.pathPending && !ReachedNPC && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            GoToNextTarget();
        }

        /* PursuitTimer -= Time.deltaTime;
        PursuitCooldown();
        if (Possessed == false && Distracted == false)
        {
            if (PursuitTimer < 0)
            {
                agent.SetDestination(target.position);
                Pursuiting = true;
            }
        }*/

        //agent.SetDestination(target.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("protectedNPC"))
        {
            //ReachedNPC = true;
            //Destroy(target.gameObject);

            // Lerp camera over to show enemy reaching NPC
           // cam.transform.position = Vector3.Lerp(transform.position, target.position + offSet, smoothSpeed * Time.deltaTime);
           // cam.transform.LookAt(target);

            // Change to the game over screen
            // SceneManager.LoadScene("GameOverScene");

            // Freeze all other objects
            // Implement a delayed game over here after camera has showed the enemy and NPC
            /*if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(3);
            }*/
            Debug.Log("The enemy has reached the NPC");
        }

        if (other.name ==("CleaverTrig"))
        {
            CleaverTrig.SetActive(false);
            CleaverProp.SetActive(true);
        }
    }

    void GoToNextTarget()
    {
        ReachedNPC = false;

        // Destroy NPCs or objects if needed
        if (targets[currentIndex] != null && targets[currentIndex].CompareTag("protectedNPC"))
        {
            Debug.Log("Enemy reached NPC!");
            Destroy(targets[currentIndex].gameObject, 0.1f);
        }

        // Move to next waypoint
        currentIndex++;

        if (currentIndex >= targets.Length)
        {
            Debug.Log("Enemy reached final target!");
            return; // Stop here, no more targets
        }

        agent.SetDestination(targets[currentIndex].position);
    }

    /* IEnumerator PursuitCooldown()
     {
         yield return new WaitForSeconds(PursuitTimer);
         //SprintTimer = SprintDuration;
         Pursuiting = true;
     }*/
}
