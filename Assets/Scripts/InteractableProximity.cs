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
        FindPlayer();
        Debug.Log($"[InteractableProximity] Start() called on {gameObject.name}, player found: {player != null}");
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"[InteractableProximity] Successfully found player: {playerObj.name}");
        }
        else
        {
            Debug.LogWarning("[InteractableProximity] Player not found! Make sure it has the 'Player' tag");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Retry finding player if it's not found yet (handles scene reload timing issues)
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (InteractionManager.Instance == null)
        {
            Debug.LogWarning($"[InteractableProximity] InteractionManager.Instance is NULL!");
            return;
        }

        DistanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If player is within range register the item as interactable and fade in the UI
        if (DistanceToPlayer <= range)
        {
            Debug.Log($"[InteractableProximity] {gameObject.name} is in range ({DistanceToPlayer}), registering with InteractionManager");
            InteractionManager.Instance.RegisterInteractable(this);
        }
    }
}
