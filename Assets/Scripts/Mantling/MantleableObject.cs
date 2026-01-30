using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MantleableObject : MonoBehaviour, IInteractable
{
    [Header("Mantle Target Offset")]
    [SerializeField] private Vector3 mantleOffset = new Vector3(0f, 2.0f, 0.5f);
    [SerializeField] private bool showGizmos = true;
    public float interactionPriority => 5f;
    //[SerializeField] private GameObject iconPrefab;
    //public bool shouldShowIcon = true;
    //private GameObject popupInstance;
    public InteractType interactType => InteractType.Mantle;

    [SerializeField] private float maxMantleDistance = 2f;
    [SerializeField] private float minHeight = 0.8f;
    [SerializeField] private float maxHeight = 1.6f;

    private void Update()
    {/*
        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }*/
    }

    public bool CanInteract(GameObject player)
    {
        if (DialogueManager.DialogueIsActive) return false;

        PlayerMantling pc = player.GetComponent<PlayerMantling>();
        if (pc == null || pc.isMantling) return false;

        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist > maxMantleDistance) return false;

        float heightDiff = transform.position.y - player.transform.position.y;
        if (heightDiff < minHeight || heightDiff > maxHeight) return false;

        Vector3 toObject = (transform.position - player.transform.position).normalized;
        if (Vector3.Dot(player.transform.forward, toObject) < 0.5f) return false;

        return true;
    }

    public void OnPlayerInteraction(GameObject player)
    {
        PlayerMantling playerMantling = player.GetComponent<PlayerMantling>();
        if (playerMantling == null) return;

        ButtonIcons.Instance?.Clear();
        playerMantling.StartMantle(this);
        //StartCoroutine(HideIconWhileMantling());
    }

    /*
    private IEnumerator HideIconWhileMantling()
    {
        DisablePopupIcon();

        yield return new WaitForSeconds(1.0f);

        EnablePopupIcon();
    }*/

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

    /*
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
    }*/
}
