using UnityEngine;
using TMPro;

public class NPCPopUpDialogue : MonoBehaviour
{
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private string dialogueLine;

    [SerializeField] private float triggerDistance = 3f;
    [SerializeField] private Transform player;

    [Header("Canvas Scaling")]
    [SerializeField] private Vector2 padding = new Vector2(20f, 20f);
    [SerializeField] private float minWidth = 100f;
    [SerializeField] private float minHeight = 50f;
    [SerializeField] private float maxWidth = 300f;

    private RectTransform canvasRect;

    private bool isShowing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueCanvas.SetActive(false);
        canvasRect = dialogueCanvas.GetComponent<RectTransform>();

        // Enable word wrapping
        dialogueText.enableWordWrapping = true;
        dialogueText.overflowMode = TextOverflowModes.Overflow;
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

        ResizeCanvas();

        dialogueCanvas.SetActive(true);
    }

    private void HideDialogue()
    {
        isShowing = false;
        dialogueCanvas.SetActive(false);
    }

    private void ResizeCanvas()
    {
        if (canvasRect == null || dialogueText == null) return;

        // Set pivot to center
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        dialogueText.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Limit width
        float width = Mathf.Min(dialogueText.preferredWidth, maxWidth);
        width = Mathf.Max(width, minWidth);

        dialogueText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        // Calculate height after wrapping
        float height = Mathf.Max(dialogueText.preferredHeight, minHeight);

        // Add Padding
        width += padding.x;
        height += padding.y;

        canvasRect.sizeDelta = new Vector2(width, height);

        // Center text inside panel
        dialogueText.rectTransform.anchoredPosition = Vector2.zero;
    }
}
