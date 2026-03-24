using UnityEngine;

public class InteractableProximity : MonoBehaviour
{
    public float range = 3f;
    public Transform player;
    public UIFadeConrtoller uiFade;

    private void Start()
    {
        uiFade = FindFirstObjectByType<UIFadeConrtoller>();
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
