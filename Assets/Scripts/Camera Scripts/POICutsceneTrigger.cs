using UnityEngine;
using System.Collections;

public class POICutsceneTrigger : MonoBehaviour
{
    [SerializeField] private GameObject VCam1;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            VCam1.SetActive(false);
           // StartCoroutine(ReEnableCam());
        }

        StartCoroutine(ReEnableCam());
        
    }


    IEnumerator ReEnableCam()
    {
        yield return new WaitForSeconds(5);
        VCam1.SetActive(true);
    }

}
