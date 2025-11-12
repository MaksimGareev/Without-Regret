using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DialogueTrigger : MonoBehaviour
{
    public string NPCName = "Friendly NPC";
    public GameObject promptUI;
    public float chatRange = 3f;
    public TextAsset jsonDialogueFile;
    private DialogueManager dialogueManager;

    private bool playerInRange = false;
    private Transform player;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Interact.performed += ctx => TryInteract();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Player reference
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Dialogue Manager
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Distance to player
        float dist = Vector3.Distance(player.position, transform.position);

        // Show/Hide prompt UI
        if (dist <= chatRange)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                if (promptUI != null)
                    promptUI.SetActive(true);
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                if (promptUI != null)
                    promptUI.SetActive(false);
            }
        }
    }

    private void TryInteract()
    {
        // Only trigger if player is close enough and not already in dialogue
        if (!playerInRange || PlayerController.DialogueActive)
            return;

       // PlayerController.DialogueActive = true;

        if (promptUI != null)
            promptUI.SetActive(false);

        // Start dialogue
        if (dialogueManager != null && jsonDialogueFile != null)
        {
            dialogueManager.StartDialogueFromJson(jsonDialogueFile);
        }
        else
        {
            Debug.LogWarning("missing Dialogue or ink Json on " + gameObject.name);
        }
    }
}
