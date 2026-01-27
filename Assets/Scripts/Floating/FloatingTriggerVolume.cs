using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloatingTriggerVolume : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;

    public float interactionPriority => 1f;
    [SerializeField] private InteractType interactType => InteractType.Float;
    //[SerializeField] private GameObject iconPrefab;
    //[SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);
    //[SerializeField] private bool shouldShowIcon = true;
    //private GameObject popupInstance;

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

    private void Update()
    {
        if (!playerInRange || playerFloating == null || ButtonIcons.Instance == null)
        {
           // Debug.LogError("PlayerFloating component is missing. Cannot update popup icon state.");
            return;
        }

        if (playerFloating.IsFloating || playerFloating.IsCoolingDown)
        {
            ButtonIcons.Instance.Clear();
        }
        else
        {
            ButtonIcons.Instance.Highlight(InteractType.Float);
        }

         /*   
        if (shouldShowIcon && popupInstance == null && !playerFloating.IsFloating && !playerFloating.IsCoolingDown)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }*/
    }

    /*
    public void EnablePopupIcon()
    {
        if (popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
            gameObject.GetComponent<WorldPopup>().worldOffset = iconOffset;
            shouldShowIcon = true;
        }
    }

    public void DisablePopupIcon()
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
            popupInstance = null;
            shouldShowIcon = false;
        }
    }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerFloating = player.GetComponent<PlayerFloating>();
            playerFloating.SetCanFloat(true);
            playerInRange = true;

            if (!playerFloating.IsFloating && !playerFloating.IsCoolingDown)
            {
                ButtonIcons.Instance?.Highlight(InteractType.Float);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerFloating.SetCanFloat(false);
            player = null;
            playerFloating = null;
            playerInRange = false;

            ButtonIcons.Instance?.Clear();
        }
    }

    public void OnDrawGizmos()
    {
        // Visualize the trigger volume
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
