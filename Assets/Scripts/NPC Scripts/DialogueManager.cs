using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject DialoguePanel;
    public TextMeshProUGUI NPCNameText;
    public TextMeshProUGUI DialogueText;
    public Transform ChoicesContainer;
    public GameObject ChoiceButton;
    private Transform playerTransform;

    private PlayerThrowing playerThrowing;
    private PlayerFloating playerFloating;

    private List<GameObject> spawnedChoices = new List<GameObject>();
    private DialogueNode currentNode;
    private string NPCName;

    void Start()
    {
        DialoguePanel.SetActive(false);
    }

    public void StartDialogue(string NPCName, DialogueNode StartNode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        playerTransform = player.transform;
        playerFloating = player.GetComponent<PlayerFloating>();
        playerThrowing = player.GetComponent<PlayerThrowing>();

        this.NPCName = NPCName;
        DialoguePanel.SetActive(true);
        PlayerController.DialogueActive = true;

        if (playerFloating != null)
        {
            playerFloating.enabled = false;
        }

        if (playerThrowing != null)
        {
            playerThrowing.enabled = false;
        }

        ShowNode(StartNode);
    }

    private void ShowNode(DialogueNode node)
    {
        currentNode = node;
        NPCNameText.text = NPCName;
        DialogueText.text = node.npcLine;

        // Clear old buttons
        foreach (var b in spawnedChoices)
            Destroy(b);
        spawnedChoices.Clear();

        // Debug check
        Debug.Log("Spawning " + node.playerChoices.Count + " choice buttons");

        // Spawn choices
        for (int i = 0; i < node.playerChoices.Count; i++)
            {
                int index = i;
                GameObject buttonObj = Instantiate(ChoiceButton, ChoicesContainer);
            buttonObj.SetActive(true); // ensure it's active
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = node.playerChoices[i];
                }
                Button btn = buttonObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnChoiceSelected(index));
                }
                spawnedChoices.Add(buttonObj);

            }

    }

    private void OnChoiceSelected(int choiceIndex)
    {
        DialogueNode nextNode = null;
        if (currentNode.nextNodes != null && choiceIndex < currentNode.nextNodes.Count)
        {
            nextNode = currentNode.nextNodes[choiceIndex];
        }
        if (nextNode != null)
        {
            ShowNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        DialoguePanel.SetActive(false);
        PlayerController.DialogueActive = false;

        if (playerFloating != null)
        {
            playerFloating.enabled = true;
        }

        if (playerThrowing != null)
        {
            playerThrowing.enabled = true;
        }

        foreach (var b in spawnedChoices)
        {
            Destroy(b);
            spawnedChoices.Clear();
        }
    }

}
