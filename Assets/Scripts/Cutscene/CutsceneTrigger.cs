using UnityEngine;


namespace Ploopploop.CMBasic
{
    public class CutsceneTrigger : MonoBehaviour
    {
        public CMCutscene cutscene;                  
        public PlayerController PlayerController; 
        public GameObject playerCam;
        public GameObject cutsceneCam;
        public GameObject playerFollow; 

        private void Start()
        {
            if (cutsceneCam != null)
                cutsceneCam.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (PlayerController != null)
                    PlayerController.enabled = false;

                if (cutsceneCam != null)
                    cutsceneCam.SetActive(true);

                if (playerCam != null)
                    playerCam.SetActive(false);

                if (playerFollow != null)
                    playerFollow.SetActive(false);

                if (cutscene != null)
                    cutscene.StartCutscene();
            }
        }

        public void EndCutscene()
        {
            if (PlayerController != null)
                PlayerController.enabled = true;

            if (cutsceneCam != null)
                cutsceneCam.SetActive(false);

            if (playerCam != null)
                playerCam.SetActive(true);

            if (playerFollow != null)
                playerFollow.SetActive(true);

            Destroy(gameObject);
        }
    }
}
