using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class DialogueNode
{
    public string npcLine;                                            // What the NPC says
    public List<string> playerChoices = new List<string>();           // Player options
    public List<DialogueNode> nextNodes = new List<DialogueNode>();   // Next node for each choice
}
public class FriendlyNPC : MonoBehaviour
{

    public NavMeshAgent agent;
    public Transform centrePoint;   // Center for wandering
    public float range = 5f;        // How far NPC wanders
    public float waitTime = 2f;
    private bool isWaiting = false;

    public string NPCName = "Friendly NPC";
    public GameObject promptUI;
    public float chatRange = 3f;

    private bool playerInRange = false;
    private Transform player;
    private DialogueManager dialogueManager;
    public DialogueNode StartNode;
    void Start()
    {
        // NavMesh setup
        agent = GetComponent<NavMeshAgent>();
        PickNewDestination();

        // Player reference
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Dialogue Manager
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        // Distance to player
        float dist = Vector3.Distance(player.position, transform.position);

        // Show/Hide prompt UI
        if (dist <= chatRange)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                if (promptUI != null)
                    promptUI.SetActive(true);
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                if (promptUI != null)
                    promptUI.SetActive(false);
            }
        }

        // NPC wandering (only if not in dialogue)
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !PlayerController.DialogueActive)
        {
            StartCoroutine(WaitBeforeNextMove());
        }

        // Start dialogue
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !PlayerController.DialogueActive)
        {
            PlayerController.DialogueActive = true;

            if (promptUI != null)
                promptUI.SetActive(false);

            dialogueManager.StartDialogue(NPCName, StartNode);

        }
    }

    // Wait before picking next wandering destination
    IEnumerator WaitBeforeNextMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        PickNewDestination();
        isWaiting = false;
    }

    // Pick a random NavMesh point to wander to
    void PickNewDestination()
    {
        Vector3 point;
        if (RandomPoint(centrePoint.position, range, out point))
        {
            agent.SetDestination(point);
        }
    }

    // Random point on NavMesh
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}
