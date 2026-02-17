using System.Collections;
using System.Linq;
using UnityEngine;

public class GraveObjectiveSlot : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Objective data for the objective this slot is linked to. This is used to check if the objective is ACTIVE and to add progress when a gravestone is placed.")]
    [SerializeField] private ObjectiveData linkedObjective;
    [Tooltip("Ghost prefab to show the player where to put the gravestone. Will be made transparent automatically.")]
    [SerializeField] private GameObject ghostPrefab;
    private Transform lockingPosition;
    private GameObject player;
    private bool isObjectiveActive = false;
    private Rigidbody rb;
    private bool didOnce = false;
    private GameObject ghostInstance;
    private float ghostAlpha = 0.7f;

    private void OnEnable()
    {
        ObjectiveManager.Instance.OnObjectiveActivated.AddListener(SetObjectiveActive);
        ObjectiveManager.Instance.OnObjectiveCompleted.AddListener(SetObjectiveInactive);
    }

    private void Start()
    {
        lockingPosition = GetComponentInChildren<Transform>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (lockingPosition == null)
        {
            Debug.LogWarning("No Transform found for locking position.");
        }

        if (player == null)
        {
            Debug.LogWarning("Player not found.");
        }

        if (linkedObjective == null)
        {
            Debug.LogWarning("No objective linked in inspector!");
        }
        
        // Check if the linked objective is already active at the start (i.e. on reloading save), reenable gravestones.
        if (linkedObjective != null && ObjectiveManager.Instance != null)
        {
            if (ObjectiveManager.Instance.IsObjectiveActive(linkedObjective.objectiveID))
            {
                SetObjectiveActive(new ObjectiveInstance(linkedObjective));
            }
        }

        if (!isObjectiveActive)
        {
            GameObject[] gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
            foreach (GameObject gravestone in gravestones)
            {
                StartCoroutine(WaitToDisableGravestone(gravestone));
            }
        }

        if (ghostPrefab != null && lockingPosition != null)
        {
            ghostInstance = Instantiate(ghostPrefab, lockingPosition.position, lockingPosition.rotation);
            MakeGhostTransparent(ghostInstance, ghostAlpha);
            ghostInstance.SetActive(false);

            Collider[] cols = ghostInstance.GetComponentsInChildren<Collider>();
            foreach (Collider col in cols)
            {
                col.enabled = false;
            }

            Rigidbody rb = ghostInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }
        }
    }

    private void Update()
    {
        GameObject[] gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
        foreach (GameObject gravestone in gravestones)
        {
            MoveableObject moveable = gravestone.GetComponent<MoveableObject>();

            if (moveable != null)
            {
                if (isObjectiveActive && moveable.IsGrabbed)
                {
                    EnableGhost();
                    return;
                }
            }
        }
        
        DisableGhost();
    }

    private IEnumerator WaitToDisableGravestone(GameObject gravestone)
    {
        yield return new WaitForSeconds(3f);
        MoveableObject moveable = gravestone.GetComponent<MoveableObject>();
        Rigidbody rb = gravestone.GetComponent<Rigidbody>();
        if (moveable != null)
        {
            moveable.isGrabbable = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isObjectiveActive)
        {
            return;
        }

        if (other.CompareTag("Gravestone"))
        {
            MoveableObject gravestone = other.GetComponent<MoveableObject>();
            rb = gravestone.gameObject.GetComponent<Rigidbody>();

            if (gravestone != null && !didOnce)
            {
                gravestone.OnPlayerInteraction(player);
                gravestone.isGrabbable = false;
                other.gameObject.transform.position = lockingPosition.position;
                other.gameObject.transform.rotation = lockingPosition.rotation;
                ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
                rb.constraints = RigidbodyConstraints.FreezeAll;
                didOnce = true;

                DisableGhost();
            }
        }
    }

    private void SetObjectiveActive(ObjectiveInstance objective)
    {
        if (objective.data != linkedObjective)
        {
            return;
        }

        isObjectiveActive = true;

        GameObject[] gravestones = GameObject.FindGameObjectsWithTag("Gravestone");
        foreach (GameObject gravestone in gravestones)
        {
            MoveableObject moveable = gravestone.GetComponent<MoveableObject>();
            rb = gravestone.GetComponent<Rigidbody>();

            if (moveable != null)
            {
                moveable.isGrabbable = true;
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        EnableGhost();
    }
    
    private void SetObjectiveInactive(ObjectiveInstance objective)
    {
        if (objective.data != linkedObjective)
        {
            return;
        }

        isObjectiveActive = false;

        DisableGhost();
    }

    private void MakeGhostTransparent(GameObject ghost, float alpha)
    {
        if (ghostInstance != null)
        {
            Renderer[] renderers = ghostInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    //Debug.Log("Material shader: " + mat.shader.name);

                    if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = alpha; // Set transparency level
                        mat.color = color;

                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                }
            }
        }
    }

    public void EnableGhost()
    {
        if (ghostInstance != null)
        {
            MakeGhostTransparent(ghostInstance, ghostAlpha);
            ghostInstance.transform.position = lockingPosition.position;
            ghostInstance.transform.rotation = lockingPosition.rotation;
            ghostInstance.SetActive(true);
        }
    }

    public void DisableGhost()
    {
        if (ghostInstance != null)
        {
            ghostInstance.SetActive(false);
        }
    }
}
