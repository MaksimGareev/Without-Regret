using UnityEngine;

public class SpawnDialogueTrigger : MonoBehaviour
{
    public GameObject dialogueTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueTrigger.SetActive(true);
        }
    }

}
