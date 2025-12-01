using UnityEngine;

public class Ending : MonoBehaviour
{
    public ScreenTransition transition;
    public GameObject PlayerUI;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Freeze player movement
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetCutsceneLocked(true); // freeze player
            }

            transition.StartTransition(); // fade to credits
            PlayerUI.SetActive(false);
        }
    }
}
