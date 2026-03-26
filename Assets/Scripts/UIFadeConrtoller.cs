using UnityEngine;

public class UIFadeConrtoller : MonoBehaviour
{
    public float fadeSpeed = 3f;
    public float idleDelay = 2f;

    private CanvasGroup canvasGroup;
    private float lastActiveTime;
    private bool isActive = true;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        lastActiveTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // check if UI should fade out
        if (Time.time - lastActiveTime > idleDelay)
        {
            isActive = false;
        }

        float targetAlpha = isActive ? 1f : 0f;

        // smooth fade
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        canvasGroup.interactable = canvasGroup.alpha > 0.5f;
        canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.5f;
    }

    public void ShowUI()
    {
        lastActiveTime = Time.time;
        isActive = true;
    }
}
