using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MantleableObject : MonoBehaviour, IInteractable
{
    [Header("Mantle Target Offset")]
    [Tooltip("Offset applied to the mantle target position. This determines where the player will be positioned when they mantle onto this object. This is visibly represented in editor as a green sphere and line.")]
    [SerializeField] private Vector3 mantleOffset = new Vector3(0f, 2.0f, 0.5f);
    
    [Tooltip("Whether to show gizmos for the mantle target position in the editor. This can help with adjusting the mantle offset to get the desired mantle position on this object.")]
    [SerializeField] private bool showGizmos = true;

    [Tooltip("Priority of this object's interaction. Lower priority objects will be interacted with first if multiple items are in range.")]
    public float interactionPriority => 5f;
    //[SerializeField] private GameObject iconPrefab;
    //public bool shouldShowIcon = true;
    //private GameObject popupInstance;
    [Tooltip("Type of interaction this object will have. This is used to determine the interaction prompt and icon that will show up when the player is in range.")]
    public InteractType interactType => InteractType.Mantle;

    [Tooltip("Maximum horizontal distance the player can be from the mantle target position to initiate a mantle. This is used to prevent mantling from too far away.")]
    [SerializeField] private float maxMantleDistance = 2f;

    [Tooltip("Minimum height difference between the player and the mantle target position to allow mantling. This is used to prevent mantling onto objects that are too low to the ground.")]
    [SerializeField] private float minHeight = 0.8f;

    [Tooltip("Maximum height difference between the player and the mantle target position to allow mantling. This is used to prevent mantling onto objects that are too high to reach.")]
    [SerializeField] private float maxHeight = 3f;

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

        Vector3 playerPos = player.transform.position;
        Vector3 mantlePos = GetMantlePosition();

        float horizontalDist = Vector3.Distance(new Vector3(playerPos.x, 0, playerPos.z), new Vector3(mantlePos.x, 0, mantlePos.z));
        if (horizontalDist > maxMantleDistance) return false;

        /*float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist > maxMantleDistance) return false;*/

       /* float heightDiff = mantlePos.y - playerPos.y;
        if (heightDiff < minHeight || heightDiff > maxHeight) return false;
       */
        Vector3 toObject = (mantlePos - playerPos).normalized;
        if (Vector3.Dot(player.transform.forward, toObject) < 0.3f) return false;

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
