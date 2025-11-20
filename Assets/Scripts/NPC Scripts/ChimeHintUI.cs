using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class ChimeHintUI : MonoBehaviour
{
    public GameObject hintBubbleUI;
    public TextMeshProUGUI hintText;
    private Transform cam;

    public float displayTime = 3f;

    private bool isShowing = false;
    private Coroutine hideRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (hintBubbleUI != null)
        {
            hintBubbleUI.SetActive(false);
        }
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) || Input.GetButtonDown("Xbox Select Button"))
        {
            ShowHint();
        }
    }

    private void LateUpdate()
    {
        Vector3 lookPos = transform.position + cam.forward;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }

    public void ShowHint()
    {
        // Get the current objective from Objective Manager
        ObjectiveInstance activeObjective = GetCurrentObjective();

        if (activeObjective == null)
        {
            hintText.text = ("I don't think there is anthing to do right now, best to keep exploring.");
        }
        else
        {
            hintText.text = activeObjective.data.description;
        }

        // Show UI
        hintBubbleUI.SetActive(true);

        // Reset timer if already counting down
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        hideRoutine = StartCoroutine(HideBubbleAfterDelay());
    }

    private IEnumerator HideBubbleAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        hintBubbleUI.SetActive(false);
        hideRoutine = null;
    }

    private ObjectiveInstance GetCurrentObjective()
    {
        foreach (var obj in ObjectiveManager.Instance.GetActiveObjectives())
        {
            return obj;
        }

        return null;
    }
}
