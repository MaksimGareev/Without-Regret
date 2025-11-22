using UnityEngine;
using System.Collections;


public class POICutsceneTrigger : MonoBehaviour
{
    [SerializeField] private GameObject VCam1;
    PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            VCam1.SetActive(false);
            playerController = other.gameObject.GetComponent<PlayerController>(); 
            playerController.MovementLocked = true;   
        }
        StartCoroutine(ReEnableCam());
        
    }
    IEnumerator ReEnableCam()
    {
        yield return new WaitForSeconds(3);
        VCam1.SetActive(true);
        
        StartCoroutine(ReEnableMovement());
    }

    IEnumerator ReEnableMovement()
    {
        yield return new WaitForSeconds(1);
        playerController.MovementLocked = false;
        playerController.enabled = true;
        gameObject.SetActive(false); 
    }
   

}
