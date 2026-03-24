using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class CheckpointTriggerVolume : MonoBehaviour
{
    [SerializeField] private bool showDebugLogs = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // When the player enters the trigger, set the checkpoint in the PlayerController and save the game
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.SetCheckpoint(SceneManager.GetActiveScene().name, transform.position, transform.eulerAngles);

            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
            
            if (showDebugLogs) Debug.Log("Checkpoint reached! Game saved.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.limeGreen;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
