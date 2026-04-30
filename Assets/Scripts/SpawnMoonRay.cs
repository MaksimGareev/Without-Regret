using UnityEngine;

public class SpawnMoonRay : MonoBehaviour
{
    public GameObject MoonRay;
    public NewDialogueTrigger DialogueTrigger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MoonRay.SetActive(true);
        }
    }

    private void Update()
    {
        if (DialogueTrigger && DialogueTrigger.completed)
        {
            MoonRay.SetActive(false);
        }
    }
}
