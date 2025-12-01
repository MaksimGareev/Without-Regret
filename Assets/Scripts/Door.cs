using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public ObjectiveData linkedObjective;
    public bool needsObjective = true;

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
        if (needsObjective)
        {
            var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
            foreach (var obj in activeObjectives)
            {
                if (obj.data == linkedObjective)
                {
                    obj.AddProgress(1);
                    StartCoroutine(WaitToLoadScene());
                    return;
                }
                else
                {
                    Debug.Log("You must complete all objectives before moving forward");
                }
            }
        }
        else
        {
            StartCoroutine(WaitToLoadScene());
        }
    }

    private IEnumerator WaitToLoadScene()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneToLoad);
    }
}

