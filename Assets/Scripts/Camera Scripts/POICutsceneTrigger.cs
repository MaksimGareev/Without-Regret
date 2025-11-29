using UnityEngine;
using System.Collections;

public class POICutsceneTrigger : MonoBehaviour
{
    [SerializeField] private GameObject VCam1;
    private PlayerController player;
    private bool triggered = false;
    private void Start()
    {
        triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !triggered)
        {
            player = other.GetComponent<PlayerController>();
            player.SetCutsceneLocked(true);

            VCam1.SetActive(false);
            StartCoroutine(ReEnableCam());
            triggered = true;
        }
    }


    IEnumerator ReEnableCam()
    {
        yield return new WaitForSeconds(3);
        VCam1.SetActive(true);
        yield return new WaitForSeconds(2);
        player.SetCutsceneLocked(false);
    }

}
