using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Darry : MonoBehaviour
{
    // Movement
    public NavMeshAgent agent;
    public Transform[] targets;
    [HideInInspector] public int currentTargetIndex = 0;
    [HideInInspector] public Transform currentTarget;
    private Coroutine waitAfterBake;

    // movemnet after dialogue
    public float Speed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates

    public Transform targetSpot;
    public Transform lookAtTarget;
    public bool isTraveling;
    public bool arrived = false;
    public float stopDistance = 0.5f;

    private float updateTimer = 0f;
    public float updateRate = 0.2f;

    public GameObject enemy;

    public Animator animator;
    public NewDialogueTrigger dialogueTrigger; // dialogue trigger script reference
    private InteractableProximity proximityScript;
    private Collider proximityCollider;

    // objectives
    [SerializeField] ObjectiveData linkedHouseObjective;
    [SerializeField] ObjectiveData linkedNeighborhoodObjective;

    private void Awake()
    {
        proximityScript = GetComponent<InteractableProximity>();
        if (proximityScript != null)
        {
            proximityCollider = proximityScript.GetComponent<Collider>();
        }
    }

    void Start()
    {
        if (targets.Length > 0)
        {
            currentTargetIndex = 0;
            currentTarget = targets[currentTargetIndex];
            agent.SetDestination(targets[currentTargetIndex].position);
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
        bool isMoving = agent.velocity.sqrMagnitude > 0.05f;

        if (animator)
        {
            animator.SetBool("isWalking", isMoving);
            animator.SetBool("isIdle", !isMoving);
        }

        // stop enemy when dialogue is active
        if (NewDialogueManager.Instance.DialogueIsActive)
        {
            agent.isStopped = true;
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            if (currentTargetIndex < targets.Length && targets[currentTargetIndex] != null)
            {
                agent.SetDestination(targets[currentTargetIndex].position);
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

        /*
        if (!agent.pathPending && agent.remainingDistance < 0.5f && agent != null)
        {
            agent.SetDestination(targets.position);
        }
        if (isTraveling)
        {
            TravelToTarget();
        }
        */
    }

    public void StartTravel()
    {
        //IsFollowing = false;
        isTraveling = true;
        Debug.Log("Darry is now traveling to her destination");
    }

    public void TravelToTarget()
    {
        if (targetSpot == null)
        {
            return;
        }

        Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        // Movement
        //transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, Speed * Time.deltaTime);
        //agent.destination = targetSpot.position;
        agent.SetDestination(currentTarget.position);

        // Rotate towards target
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }

        // Stop when close to target destination
        if (Vector3.Distance(transform.position, targetSpot.position) < stopDistance)
        {
            isTraveling = false;
            arrived = true;
            Debug.Log("Darry reached the destination.");
        }
    }

    void GoToNextTarget()
    {
        //Debug.Log("Going to next point");
        StopWaitCoroutine();
        // ReachedNPC = false;

        /*  // Destroy NPCs or objects if needed
          if (targets[currentIndex] != null && targets[currentIndex].CompareTag("protectedNPC") || targets[currentIndex].CompareTag("Darry"))
          {
              Debug.Log("Enemy reached NPC!");
              Destroy(targets[currentIndex].gameObject, 0.1f);
          }*/

        // Move to next waypoint
        currentTargetIndex++;

        waitAfterBake = StartCoroutine(waitForNavmesh()); //Waits for navmesh to be baked before moving
        if (currentTargetIndex >= targets.Length)
        {
            //Debug.Log("Darry reached final target!");
            currentTarget = null;       // <--- set to null when no more targets
            agent.isStopped = true;     // stop the NavMeshAgent

            StopWaitCoroutine();

            return; // Stop here, no more targets
        }

        currentTarget = targets[currentTargetIndex];
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }
    public void StartDissolve(float duration)
    {
        StartCoroutine(DissolveOut(duration));
    }

    IEnumerator DissolveOut(float duration)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float time = 0f;

        // switch materials to transparent to activate fade
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                // Change surface type to transparent so alpha will work
                if (mats[i].HasProperty("_Surface"))
                {
                    mats[i].SetFloat("_Surface", 1f);
                }

                // Ensure rendering mode updates correctly
                mats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mats[i].SetInt("_ZWrite", 0);
                mats[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mats[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }

        // store original colors
        Color[][] originalColors = new Color[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            originalColors[i] = new Color[mats.Length];
            for (int j = 0; j < mats.Length; j++)
            {
                originalColors[i][j] = mats[j].color;
            }
        }

        while (time < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / duration);

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    Color c = originalColors[i][j];
                    c.a = alpha;
                    mats[j].color = c;
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        // ensure fully invisible at the end
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                Color c = originalColors[i][j];
                c.a = 0f;
                mats[j].color = c;
            }
        }

        gameObject.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            StopWaitCoroutine();
            agent.enabled = false;
            StartCoroutine(DissolveOut(1.5f));

            if (linkedHouseObjective != null)
            {
                ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            }
            this.gameObject.SetActive(false);
            Debug.Log("Darry has reached the door.");
        }

        if (other.CompareTag("Finish"))
        {
            StopWaitCoroutine();
            if(enemy != null)
            {
                enemy.SetActive(false);
            }
            if (linkedNeighborhoodObjective != null)
            {
                ObjectiveManager.Instance.AddProgress(linkedNeighborhoodObjective.objectiveID, 1);
            }
            Debug.Log("Darry has made it to the end.");

            if (dialogueTrigger != null && !dialogueTrigger.enabled)
            {
                dialogueTrigger.enabled = true;

                Collider col = dialogueTrigger.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = true;
                }
                Debug.Log("Irene's dialogue trigger has been deactivated.");
            }
        }
    }

    IEnumerator waitForNavmesh()
    {
        yield return new WaitForSeconds(1.0f);
    }
    private void StopWaitCoroutine() //stops existing WaitForNavmesh coroutine
    {
        if (waitAfterBake != null)
        {
            StopCoroutine(waitAfterBake);
            waitAfterBake = null;
        }
    }
}
