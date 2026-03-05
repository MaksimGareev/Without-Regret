using System.Collections;
using UnityEngine;

public class TreeSequenceController : MonoBehaviour
{
    [Header("Echo (Player)")]
    public Transform echo; //drag player
    public float triggerDistance = 2f;

    [Header("Fade")]
    public FadeController fade;
    public float blackHoldTime = 0.1f;

    [Header("Hazards")]
    public HazardSpawner hazardSpawner;

    [Header("Human Chime")]
    public GameObject humanChimePrefab;
    public Transform humanSpawnPoint;
    public GameObject echoRootToDisable; // Player root
    public float timeBetweenFades = 5f; // Delay before the 2nd fade

    private bool started = false;

    void Update()
    {
        if (started) return;
        if (echo == null) return;

        float dist = Vector3.Distance(echo.position, transform.position);
        if (dist <= triggerDistance)
        {
            started = true;
            StartCoroutine(Sequence());
        }
    }

    private IEnumerator Sequence()
    {
        
        fade.StartFadeWithBlackEvent(() => //Fade hazard
        {
            hazardSpawner.SpawnHazards();
        }, blackHoldTime);

        
        yield return new WaitForSeconds(fade.fadeDuration * 2f + blackHoldTime); // Waits for fade to fully finish

        
        yield return new WaitForSeconds(timeBetweenFades);  // pacing

        
        fade.StartFadeWithBlackEvent(() => //Fade 2: spawn Human Chime / player swaps to chime
        {
            if (echoRootToDisable != null)
                echoRootToDisable.SetActive(false);

            Instantiate(humanChimePrefab, humanSpawnPoint.position, humanSpawnPoint.rotation);
        }, blackHoldTime);

        yield return new WaitForSeconds(fade.fadeDuration * 2f + blackHoldTime);
    }
}