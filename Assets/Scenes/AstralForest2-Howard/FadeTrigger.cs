using UnityEngine;

public class FadeTrigger : MonoBehaviour
{
    public FadeController fadeController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fadeController.StartFade();
        }
    }
}
