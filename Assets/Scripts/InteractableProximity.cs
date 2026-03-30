using UnityEngine;

public class InteractableProximity : MonoBehaviour
{
    public float range = 3f;
    public Transform player;

    public float DistanceToPlayer { get; private set; }

    private void Start()
    {
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

        if (DistanceToPlayer <= range)
        {
            InteractionManager.Instance?.RegisterInteractable(this);
        }
    }
}
