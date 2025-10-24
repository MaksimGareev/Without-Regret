using UnityEngine;

[DisallowMultipleComponent]
public class MantleableObject : MonoBehaviour, IInteractable
{
    [Header("Mantle Target Offset")]
    [SerializeField] private Vector3 mantleOffset = new Vector3(0f, 1.0f, 0.5f);
    [SerializeField] private bool showGizmos = true;
    public float interactionPriority => 1f;

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
