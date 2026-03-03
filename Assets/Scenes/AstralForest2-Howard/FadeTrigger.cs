using UnityEngine;

public class FadeTrigger : MonoBehaviour
{
    public FadeController fadeController;
    public HazardSpawner hazardSpawner;

    private bool hasRun = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasRun) return;
        if (!other.CompareTag("Player")) return;

        hasRun = true;

        fadeController.StartFadeWithBlackEvent(() =>
        {
            hazardSpawner.SpawnHazards();
        }, 0.1f);
    }
}