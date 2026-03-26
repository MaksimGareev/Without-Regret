using UnityEngine;

public class InteractableProximity : MonoBehaviour
{
    public float range = 3f;
    public Transform player;
    public UIFadeConrtoller uiFade;

    private void Start()
    {
        uiFade = FindFirstObjectByType<UIFadeConrtoller>();

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
        if (uiFade == null || player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= range)
        {
            uiFade.ShowUI();
        }
    }
}
