using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    //scene to load
    public string sceneToLoad;
    // distance to interact        
    public float interactDistance = 3f;
    // player
    public Transform player;

    [Header("Audio Settings")]

    public AudioClip interactSound;
    //public AudioSource audioSource; not sure if needed ,but will keep for now

    private bool isPlayerNear = false;
    private bool isInteracting = false;

    private void Start()
    {

    }

    void Update()
    {
       
        if (player != null && Vector3.Distance(player.position, transform.position) <= interactDistance)
        {
            isPlayerNear = true;

            // if player presses E to enter
            if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Xbox X Button"))
            {
                LoadScene();
            }
        }
        else
        {
            isPlayerNear = false;
        }
    }

    void LoadScene()
    {
        if (ObjectiveManager.Instance.AllObjectivesCompletedInScene())
        {
            // Load the  scene  added in Build Settings
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("You must complete all objectives before moving forward");
        }
    }
}

