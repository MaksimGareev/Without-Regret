using UnityEngine;

public class UIFadeController : MonoBehaviour
{
    public float fadeSpeed = 3f;
    public float idleDelay = 2f;

    public static UIFadeController Instance;

    private CanvasGroup canvasGroup;
    private float lastActiveTime;
    private bool isActive = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup missing on UIFadeController!");
        }

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
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        canvasGroup.interactable = canvasGroup.alpha > 0.5f;
        canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.5f;
    }

    public void ShowUI()
    {
        lastActiveTime = Time.time;
        isActive = true;
    }
}
