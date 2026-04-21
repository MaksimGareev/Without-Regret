using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Irene : MonoBehaviour
{
    public Animator animator;
    [HideInInspector] public Transform player;      // the player to follow
    public string npcName = "Irene";                // string data of npc name
    public float FollowDistance = 2f;               // how far behind the player
    public float FollowSpeed = 3f;                  // movement speed
    public float RotationSpeed = 3f;                // how fast the NPC rotates
    public float BaseSpeed = 3.5f;                  // how fast the NPC is
    public float SprintingSpeed = 6f;               // how fast the NPC is when the player is too far away
    public bool IsFollowing = false;
    private bool isMoving = false;                  //detects whether or not Irene is moving for animator purposes
    private Vector3 lastPosition;

    public NewDialogueTrigger dialogueTrigger;      // dialogue trigger script reference

    public Transform targetSpot;
    public Transform GoBackHomeSpot;
    public Transform lookAtTarget;
    private NavMeshAgent agent;
    public bool isTraveling;
    public bool isTalking = false;
    public bool arrived = false;
    public bool CanFollowPlayer = true;
    public float stopDistance = 0.5f;

    private InteractableProximity proximityScript;
    private Collider proximityCollider;

    private void Awake()
    {
        proximityScript = GetComponent<InteractableProximity>();
        if (proximityScript != null)
        {
            proximityCollider = proximityScript.GetComponent<Collider>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = FollowDistance;
        //agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        movementAnimation();

        bool shouldDisableInteraction = IsFollowing || isTraveling;

        if (dialogueTrigger != null)
        {
            dialogueTrigger.enabled = !shouldDisableInteraction;
            /*
            Collider col = dialogueTrigger.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = !shouldDisableInteraction;
            }
            */
        }

        if (proximityScript != null)
        {
            proximityScript.enabled = !shouldDisableInteraction;
        }

        if (proximityCollider != null)
        {
            proximityCollider.enabled = !shouldDisableInteraction;
        }

        if (IsFollowing)
        {
            Follow();
        }

        if (isTraveling)
        {
            TravelToTarget();
        }
        /*
        if (IsFollowing == true)
        {
            Follow();

            // disable dialogue trigger when following
            if (dialogueTrigger != null && dialogueTrigger.enabled)
            {
                dialogueTrigger.enabled = false;

                Collider col = dialogueTrigger.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
                Debug.Log("Irene's dialogue trigger has been deactivated.");
            }

            if (proximityScript != null && proximityScript.enabled)
            {
                proximityScript.enabled = false;

                if (proximityCollider != null)
                {
                    proximityCollider.enabled = false;
                }
            }

        }
        else
        {
            if (proximityScript != null && !proximityScript.enabled)
            {
                proximityScript.enabled = true;

                if (proximityCollider != null)
                {
                    proximityCollider.enabled = true;
                }
            }
        }

        if (isTraveling)
        {
            TravelToTarget();

            // disable dialogue trigger when following
            if (dialogueTrigger != null && dialogueTrigger.enabled)
            {
                dialogueTrigger.enabled = false;

                Collider col = dialogueTrigger.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
                Debug.Log("Irene's dialogue trigger has been deactivated.");
            }

            if (proximityScript != null && proximityScript.enabled)
            {
                proximityScript.enabled = false;

                if (proximityCollider != null)
                {
                    proximityCollider.enabled = false;
                }
            }
        }
        else
        {
            if (proximityScript != null && !proximityScript.enabled)
            {
                proximityScript.enabled = true;

                if (proximityCollider != null)
                {
                    proximityCollider.enabled = true;
                }
            }
        }

        if (isTalking)
        {
            dialogueTrigger.isLookingAtPlayer = true;
        }
        */

        if (arrived && lookAtTarget != null)
        {
            LookAtObject();
        }
    }

    public void Follow()
    {
        animator.SetBool("isTalking", false);
        if (dialogueTrigger != null)
        {
            dialogueTrigger.StopLookingAtPlayer();
        }
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); //calculates distance from the player as NPC is following

        if (distanceToPlayer >= FollowDistance + 5.0f)
        {
            if (agent.speed != SprintingSpeed)
            {
                agent.speed = SprintingSpeed;
                animator.speed = 2;
                // Debug.Log("Irene is too far!");
            }
        }
        else
        {
            // Reset speed
            if (agent.speed != BaseSpeed)
            {
                agent.speed = BaseSpeed;
                animator.speed = 1;

            }
        }

        if (distanceToPlayer <= FollowDistance + 1.5f) //stops the NPC from following the player when they are too close
        {
            agent.ResetPath();
            return;
        }

        agent.SetDestination(player.position);

        Vector3 LookDirection = player.position - transform.position;
        LookDirection.y = 0f;

        if (LookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(LookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
       
    }

    public void TravelToTarget()
    {
        animator.SetBool("isTalking", false);
        if (dialogueTrigger != null)
        {
            dialogueTrigger.StopLookingAtPlayer();
        }
        if (targetSpot == null || agent == null)
        {
            Debug.Log("There is no target for Irene to go to");
            return;
        }

        // Movement
        //transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, FollowSpeed * Time.deltaTime);
        if (!agent.hasPath)//(!agent.pathPending && agent.remainingDistance < 0.5f && agent != null)
        {
            agent.SetDestination(targetSpot.position);
        }

        // Rotate towards target
        /*Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }*/

        // Stop when close to target destination
        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            isTraveling = false;
            arrived = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            ReactivateDialogue(); ; // enable dialogue trigger upon arrival
            Debug.Log("Irene reached the destination.");
        }
    }

    private void ReactivateDialogue()
    {
        if (dialogueTrigger == null) return;

        dialogueTrigger.enabled = true;

        Collider col = dialogueTrigger.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        Debug.Log("Irene's dialogue trigger has been reactivated");
    }

    public void StartTravel()
    {
        CanFollowPlayer = false;
        IsFollowing = false;
        isTraveling = true;
        dialogueTrigger.isLookingAtPlayer = false;
        arrived = false;
        Debug.Log("Irene is now traveling to her destination");
    }

    public void LookAtObject()
    {
        Vector3 direction = lookAtTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
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
            agent.enabled = false;
            StartCoroutine(DissolveOut(1.5f));
            //ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            Debug.Log("Irene has reached the door.");
        }
    }

    public bool NPCNameMatches(string name)
    {
        return string.Equals(npcName, name, System.StringComparison.OrdinalIgnoreCase);
    }

    public void movementAnimation() //handles movement animations
    {
        if (arrived)
        {
            isMoving = false;
        }
        isMoving = agent.velocity.sqrMagnitude > 0.05f; //detects whether Irene is moving to play idle or walking animations

        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isIdle", !isMoving);

        //lastPosition = transform.position;
    }
}
