using UnityEngine;

// Marks an item as interactable to trigger UI fade in
public class InteractableProximity : MonoBehaviour
{
    public float range = 5f;    // How close the player needs to be for UI to appear
    public Transform player;    // Player reference

    public float DistanceToPlayer { get; private set; }

    private void Start()
    {
        // Find the player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure it has the 'Player' tag");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        DistanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If player is within range register the item as interactable and fade in the UI
        if (DistanceToPlayer <= range)
        {
            InteractionManager.Instance?.RegisterInteractable(this);
        }
    }
}
