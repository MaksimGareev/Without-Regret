using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloatingTriggerVolume : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool shouldShowIcon = true;
    private GameObject popupInstance;

    
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
        if (playerFloating == null)
        {
            //Debug.LogError("PlayerFloating component is missing. Cannot update popup icon state.");
            return;
        }
            
        if (shouldShowIcon && popupInstance == null && !playerFloating.IsFloating && !playerFloating.IsCoolingDown)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerFloating = player.GetComponent<PlayerFloating>();
            playerFloating.SetCanFloat(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerFloating.SetCanFloat(false);
            player = null;
            playerFloating = null;
        }
    }

    public void OnDrawGizmos()
    {
        // Visualize the trigger volume
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
