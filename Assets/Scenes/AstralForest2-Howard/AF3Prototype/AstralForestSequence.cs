using System.Collections;
using UnityEngine;

public class TreeVoidSequence : MonoBehaviour
{
    [Header("References")]
    public Transform player; // Player
    public GameObject voidPoolPrefab; //Void Pool Prefab
    private GameObject voidPoolPrefab_instance; //holds the instance 

    [Header("Distance")]
    public float triggerDistance = 8f; // Distance to trigger the voidpool pull //set to 20 on inspector

    [Header("Void Pull")]
    public float pullSpeed = 6f; // Pull down speed on the player
    public float poolYOffset = 0f; // Adjust vertical spawn position of pool
    public float cutToBlackDelay = 1.5f; // Delay before fade-in black screen

    [Header("Timing")]
    public float blackScreenDuration = 5f; // Timer on Black screen

    [Header("Player Control")]
    public MonoBehaviour playerControllerScript;
    public CharacterController characterController;

    [Header("UI")]
    public CanvasGroup instantBlackScreen; 
    public ScreenFadeController fadeController; // Handles fade-in

    private bool sequenceStarted = false; // Disables multiple triggers

    void Start()  // Black screen on hidden at start
    {
        if (instantBlackScreen != null)
        {
            instantBlackScreen.alpha = 0f;
            instantBlackScreen.interactable = false;
            instantBlackScreen.blocksRaycasts = false;
        }
    }

    void Update() // Checks Player Distance // Does nothing if already triggered or player is missing 
    {
        if (sequenceStarted || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position); // Distance between tree and player 

        if (distance <= triggerDistance) // Starts the sequence when player is close to the tree 
        {
            StartCoroutine(VoidSequence());
        }
    }

    IEnumerator VoidSequence()  // Spawns void under player 
    {
        sequenceStarted = true;

        Vector3 poolPos = new Vector3(
            player.position.x,
            player.position.y + poolYOffset,
            player.position.z
        );

        voidPoolPrefab_instance = Instantiate(voidPoolPrefab, poolPos, Quaternion.identity); // Disables Player movement 

        if (playerControllerScript != null)
            playerControllerScript.enabled = false;

        if (characterController != null) // VoidPool spawn under player 
            characterController.enabled = false;

        VoidPull pull = player.gameObject.AddComponent<VoidPull>();
        pull.pullSpeed = pullSpeed; // Wait so player can see the voidpool spawn

        yield return new WaitForSeconds(cutToBlackDelay); // Instant cut to black screen 

        if (instantBlackScreen != null)
        {
            instantBlackScreen.alpha = 1f; // Black screen for a second 
            instantBlackScreen.interactable = true;
            instantBlackScreen.blocksRaycasts = true;
        }

        yield return new WaitForSeconds(blackScreenDuration);

        if (pull != null)
            Destroy(pull);

        if (voidPoolPrefab_instance)
        {
            Destroy(voidPoolPrefab_instance);
        }

        if (characterController != null) // Re-enable player controller 
            characterController.enabled = true;

        if (playerControllerScript != null)
            playerControllerScript.enabled = true;

        if (fadeController != null) // Fade black screen into gameplay
        {
            fadeController.FadeFromBlack();
        }
        else if (instantBlackScreen != null)
        {
            instantBlackScreen.alpha = 0f;
            instantBlackScreen.interactable = false;
            instantBlackScreen.blocksRaycasts = false;
        }
    }
}