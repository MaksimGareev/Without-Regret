using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FloatingTriggerVolume : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject floatPromptUI;

    private GameObject player;
    private PlayerFloating playerFloating;

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
