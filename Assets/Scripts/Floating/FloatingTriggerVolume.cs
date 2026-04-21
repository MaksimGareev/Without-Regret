using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloatingTriggerVolume : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject player;

    //[SerializeField] private GameObject iconPrefab;
    //[SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);
    //[SerializeField] private bool shouldShowIcon = true;
    //private GameObject popupInstance;
    public InteractType interactType => InteractType.Float;
    public float interactionPriority => 4f;

    private bool playerInRange = false;
    private PlayerFloating playerFloating;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerFloating = player.GetComponent<PlayerFloating>();
            if(playerFloating == null)
            {
                Debug.LogError("PlayerFloating component not found on the Player GameObject.");
            }
        }
        else
        {
            Debug.LogError("Player not found in the scene. Please ensure there is a GameObject tagged 'Player'.");
        }
    }

    public bool CanInteract(GameObject player)
    {
        if (player == null) return false;

        var interacting = player.GetComponent<PlayerInteracting>();
        //bool dialogue = NewDialogueManager.Instance.DialogueIsActive;
        if (interacting != null && interacting.IsHoldingObject())
        {
            return false;
        }

        if (playerFloating == null)
        {
            playerFloating = player.GetComponent<PlayerFloating>();
        }

        if (playerFloating == null)
        {
            return false;
        }

        // Only allow interaction if not already floating or cooling down
        return !playerFloating.IsFloating && !playerFloating.IsCoolingDown;
    }

    public void OnPlayerInteraction(GameObject player)
    {
        if (playerFloating == null)
        {
            playerFloating = player.GetComponent<PlayerFloating>();
        }

        if (playerFloating == null) return;

        //playerFloating.StartFloating();
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerFloating = player.GetComponent<PlayerFloating>();

            var interacting = player.GetComponent<PlayerInteracting>();

            if (interacting != null && interacting.IsHoldingObject())
            {
                playerFloating.SetCanFloat(false);
                return;
            }

            playerFloating.SetCanFloat(true);
            playerInRange = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var interacting = other.GetComponent<PlayerInteracting>();

            if (interacting != null && interacting.IsHoldingObject())
            {
                playerFloating.SetCanFloat(false);
            }
            else
            {
                playerFloating.SetCanFloat(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerFloating.SetCanFloat(false);
            playerInRange = false;
        }
    }
    

    public void OnDrawGizmos()
    {
        // Visualize the trigger volume
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
