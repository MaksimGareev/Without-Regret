using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloatingTriggerVolume : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private bool shouldShowIcon = true;
    private GameObject popupInstance;

    private GameObject player;
    private PlayerFloating playerFloating;

    private void Start()
    {
        popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
    }

  private void Update()
    {
        if (player == null || playerFloating == null)
            return;

        if (shouldShowIcon && popupInstance != null && !popupInstance.activeSelf && !playerFloating.isFloating && !playerFloating.isCoolingDown)
        {
            popupInstance.SetActive(true);
        }
        else if (!shouldShowIcon && popupInstance != null && popupInstance.activeSelf)
        {
            popupInstance.SetActive(false);
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
}
