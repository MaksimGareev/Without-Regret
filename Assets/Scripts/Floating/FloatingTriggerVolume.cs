using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloatingTriggerVolume : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject floatPromptUI;
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
        if (player != null && !playerFloating.isCoolingDown && !playerFloating.isFloating)
        {
            floatPromptUI.SetActive(true);
        }
        else
        {
            floatPromptUI.SetActive(false);
        }

        if (shouldShowIcon && !popupInstance.activeSelf)
        {
            popupInstance.SetActive(true);
        }
        else if (!shouldShowIcon && popupInstance.activeSelf)
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

            if (floatPromptUI != null)
            {
                floatPromptUI.SetActive(true);
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

            if (floatPromptUI != null)
            {
                floatPromptUI.SetActive(false);
            }
        }
    } 
}
