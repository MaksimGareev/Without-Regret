using UnityEngine;

[DisallowMultipleComponent]
public class MantleableObject : MonoBehaviour, IInteractable
{
    [Header("Mantle Target Offset")]
    [SerializeField] private Vector3 mantleOffset = new Vector3(0f, 1.0f, 0.5f);
    [SerializeField] private bool showGizmos = true;
    public float interactionPriority => 1f;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private bool shouldShowIcon = true;
    private GameObject popupInstance;
    

    private void Start()
    {
        popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
    }

    private void Update()
    {
        if (shouldShowIcon && !popupInstance.activeSelf)
        {
            popupInstance.SetActive(true);
        }
        else if (!shouldShowIcon && popupInstance.activeSelf)
        {
            popupInstance.SetActive(false);
        }
    }

    public void OnPlayerInteraction(GameObject player)
    {
        PlayerMantling playerMantling = player.GetComponent<PlayerMantling>();
        playerMantling.StartMantle(this);
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
}
