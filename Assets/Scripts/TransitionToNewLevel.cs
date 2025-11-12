using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToNewLevel : MonoBehaviour
{
    public string sceneToLoad;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
