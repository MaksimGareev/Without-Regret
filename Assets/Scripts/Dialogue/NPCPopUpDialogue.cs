using UnityEngine;
using TMPro;

public class NPCPopUpDialogue : MonoBehaviour
{
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private string dialogueLine;

    [SerializeField] private float triggerDistance = 3f;
    [SerializeField] private Transform player;

    private bool isShowing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance && !isShowing)
        {
            ShowDialogue();
        }
        else if (distance > triggerDistance && isShowing)
        {
            HideDialogue();
        }
    }

    private void ShowDialogue()
    {
        isShowing = true;
        dialogueText.text = dialogueLine;
        dialogueCanvas.SetActive(true);
    }

    private void HideDialogue()
    {
        isShowing = false;
        dialogueCanvas.SetActive(false);
    }
}
