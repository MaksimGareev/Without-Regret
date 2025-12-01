using UnityEngine;

public class CutsceneEventBridge : MonoBehaviour
{
    public PlayerController player;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }
    }

    public void LockPlayer()
    {
        if (player != null)
            player.SetCutsceneLocked(true);
    }

    public void UnlockPlayer()
    {
        if (player != null)
            player.SetCutsceneLocked(false);
    }
}
