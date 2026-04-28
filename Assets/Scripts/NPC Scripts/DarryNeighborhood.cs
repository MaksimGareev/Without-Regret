using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class DarryNeighborhood : MonoBehaviour
{
    // movemnet after dialogue
    public float Speed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates

    public NavMeshAgent agent;
    public Transform targetSpot;
    public Transform lookAtTarget;
    public bool isTraveling;
    public bool arrived = false;
    public float stopDistance = 0.5f;
    public GameObject IntruderTrigger;
    public Animator animator;

    public string npcName = "Darry";

    // objectives
    [SerializeField] ObjectiveData linkedHouseObjective;
    [SerializeField] ObjectiveData linkedNeighborhoodObjective;
    
    [Tooltip("The objective that should cause the intruder triggers to spawn when it becomes complete.")]
    [SerializeField] private ObjectiveData mediationObjective;
    
    private bool triggersActivated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!CheckIfObjectiveComplete())
        {
            IntruderTrigger.SetActive(false);
            triggersActivated = false;
        }
        else
        {
            IntruderTrigger.SetActive(true);
            triggersActivated = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!triggersActivated && mediationObjective && CheckIfObjectiveComplete())
        {
            IntruderTrigger.SetActive(true);
            triggersActivated = true;
        }
        
        if (!isTraveling) return;
        
        TravelToTarget();
        bool isMoving = agent.velocity.sqrMagnitude > 0.05f;

        if (animator)
        {
            animator.SetBool("isWalking", isMoving);
            animator.SetBool("isIdle", !isMoving);
        }
        
        /*else if (arrived && lookAtTarget != null)
        {
            LookAtObject();
        }*/
    }

    private bool CheckIfObjectiveComplete()
    {
        if (!mediationObjective) return false;

        if (ObjectiveManager.Instance)
        {
            return ObjectiveManager.Instance.IsObjectiveCompleted(mediationObjective.objectiveID);
        }

        return false;
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
            Debug.Log("There is no target for Darry to go to");
            return;
        }

        Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        // Movement
        //transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, Speed * Time.deltaTime);

        if (!agent.pathPending && agent.remainingDistance < 0.005f && agent != null)
        {
            agent.SetDestination(targetSpot.position);
        }

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
            arrived = true;
            //ObjectiveManager.Instance.AddProgress(linkedHouseObjective.objectiveID, 1);
            Debug.Log("Darry has reached the door.");
        }

        if (other.CompareTag("Finish"))
        {
            ObjectiveManager.Instance.AddProgress(linkedNeighborhoodObjective.objectiveID, 1);
            Debug.Log("Darry has made it to the end.");
        }
    }
}
