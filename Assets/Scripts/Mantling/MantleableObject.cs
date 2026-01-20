using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MantleableObject : MonoBehaviour, IInteractable
{
    [Header("Mantle Target Offset")]
    [SerializeField] private Vector3 mantleOffset = new Vector3(0f, 2.0f, 0.5f);
    [SerializeField] private bool showGizmos = true;
    public float interactionPriority => 1f;
    [SerializeField] private GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

    private void Update()
    {
        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }
    }

    public void OnPlayerInteraction(GameObject player)
    {
        PlayerMantling playerMantling = player.GetComponent<PlayerMantling>();
        playerMantling.StartMantle(this);
        StartCoroutine(HideIconWhileMantling());
    }

    private IEnumerator HideIconWhileMantling()
    {
        DisablePopupIcon();

        yield return new WaitForSeconds(1.0f);

        EnablePopupIcon();
    }

    public Vector3 GetMantlePosition()
    {
        return transform.position + transform.TransformDirection(mantleOffset);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetMantlePosition(), 0.1f);
        Gizmos.DrawLine(transform.position, GetMantlePosition());
    }

    public void EnablePopupIcon()
    {
        if (popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
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
}
