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

    // Player morality effects
    public float baseSpeed = 2f;
    public float minSpeed = 1.5f;
    public float maxSpeed = 6.5f;
    public float moralitySpeedMultiplier = 0.15f;
    public DialogueManager playerMorality;

    // cleaver pickup
    public GameObject CleaverTrig;
    public GameObject CleaverProp;

    // Camera
    public Camera cam;
    public Vector3 offSet;
    public float smoothSpeed = 5f;

    // Check if enemy reached NPC
    public bool ReachedNPC = false;

    [HideInInspector] public Transform currentTarget;

    // Check if the enemy is possessed or distracted
    public bool Possessed = false;
    public bool Distracted = false;

    // How often to update NavMeshAgent destination (seconds)
    public float updateRate = 0.2f;
    private float updateTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerMorality == null)
        {
            playerMorality = FindObjectOfType<DialogueManager>();
        }

        agent.speed = baseSpeed;

        //StoppingDistance = new Vector3(.7f, .7f, .7f);
        if (targets.Length > 0)
        {
            currentIndex = 0;
            currentTarget = targets[currentIndex];
            agent.SetDestination(targets[currentIndex].position);
        }
        else
        {
            currentTarget = null;
            Debug.LogWarning("No targets assigned to ChasingEnemy!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpeedFromMorality();

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

        // Update agent destination periodically
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            if (targets[currentIndex] != null)
            {
                agent.SetDestination(targets[currentIndex].position);
            }
            updateTimer = updateRate;
        }


        // Go to next target after reaching current target
        if (!agent.pathPending)
        {
            if (agent.remainingDistance != Mathf.Infinity &&
                agent.remainingDistance <= agent.stoppingDistance &&
                agent.velocity.sqrMagnitude < 0.1f)
            {
                GoToNextTarget();
            }
        }

        //agent.SetDestination(target.position);
    }

    public void UpdateSpeedFromMorality()
    {
        if (playerMorality == null) return;

        int morality = playerMorality.playerMorality;

        float speedOffset = -morality * moralitySpeedMultiplier;

        float newSpeed = baseSpeed + speedOffset;

        agent.speed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Darry"))
        {
            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(3);
            }
            //ReachedNPC = true;
            //Destroy(target.gameObject);

            // Lerp camera over to show enemy reaching NPC
           // cam.transform.position = Vector3.Lerp(transform.position, target.position + offSet, smoothSpeed * Time.deltaTime);
           // cam.transform.LookAt(target);

            // Change to the game over screen
            // SceneManager.LoadScene("GameOverScene");

            // Freeze all other objects
            // Implement a delayed game over here after camera has showed the enemy and NPC
            if (TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(3);
            }
            Debug.Log("The enemy has reached the NPC");
        }

        if (other.name ==("CleaverTrig"))
        {
            CleaverTrig.SetActive(false);
            CleaverProp.SetActive(true);
        }

        if (other.name == ("EnemyEndPoint"))
        {
            this.gameObject.SetActive(false);
        }
    }

    void GoToNextTarget()
    {
       // ReachedNPC = false;

      /*  // Destroy NPCs or objects if needed
        if (targets[currentIndex] != null && targets[currentIndex].CompareTag("protectedNPC") || targets[currentIndex].CompareTag("Darry"))
        {
            Debug.Log("Enemy reached NPC!");
            Destroy(targets[currentIndex].gameObject, 0.1f);
        }*/

        // Move to next waypoint
        currentIndex++;

        if (currentIndex >= targets.Length)
        {
            Debug.Log("Enemy reached final target!");
            currentTarget = null;       // <--- set to null when no more targets
            agent.isStopped = true;     // stop the NavMeshAgent
            return; // Stop here, no more targets
        }

        currentTarget = targets[currentIndex];
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }

    /* IEnumerator PursuitCooldown()
     {
         yield return new WaitForSeconds(PursuitTimer);
         //SprintTimer = SprintDuration;
         Pursuiting = true;
     }*/
}
