using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Barry : MonoBehaviour
{
    public float Speed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates
    public NewDialogueTrigger dialogueTrigger;
    public Transform targetSpot;
    public Transform lookAtTarget;
    public bool isTraveling = false;
    public bool arrived = false;
    public float stopDistance = 0.5f;
    public NavMeshAgent agent;

    public Animator animator;

    public string npcName = "Barry";

    private void Awake()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                // Create a unique instance for runtime changes
                mats[i] = new Material(mats[i]);
            }
            r.materials = mats;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // agent.updateRotation = false;

        if (!animator)
        {
            Debug.LogError($"{this.name} has no animator assigned to the Barry script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTraveling)
        {
            TravelToTarget();
            bool isMoving = agent.velocity.sqrMagnitude > 0.05f;
            
            if (animator)
            {
                animator.SetBool("isWalking", isMoving);
                animator.SetBool("isIdle", !isMoving);
            }
        }
        /*else if (arrived && lookAtTarget != null)
        {
            LookAtObject();
        }*/
    }
    public void StartTravel()
    {
        if (!targetSpot || !agent) return;
        
        if (animator)
        {
            animator.SetBool("isTalking", false);
        }
        
        //IsFollowing = false;
        isTraveling = true;
        arrived = false;
        if (dialogueTrigger != null)
        {
            dialogueTrigger.isLookingAtPlayer = false;
        }
        
        agent.SetDestination(targetSpot.position);
        Debug.Log("Barry is now traveling to her destination");
    }

    public void TravelToTarget()
    {
        if (targetSpot == null)
        {
            Debug.Log("There is no target for Barry to go to");
            return;
        }

        Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        // Movement
        //transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, Speed * Time.deltaTime);
        /*
        if (!agent.pathPending && agent.remainingDistance < 0.5f && agent != null)
        {
            agent.SetDestination(targetSpot.position);
        }
        */
        // Rotate towards target
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }

        // Stop when close to target destination
        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            isTraveling = false;
            arrived = true;
            Debug.Log("Barry reached the destination.");
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
            for (int i = 0; i <mats.Length; i++)
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
            Debug.Log("Barry has reached the door.");
        }
    }

   public bool NPCNameMatches(string name)
    {
        return string.Equals(npcName, name, System.StringComparison.OrdinalIgnoreCase);
    }
}
