using UnityEngine;
using UnityEngine.SceneManagement;

// UI script that controls the functionality of the UI fade in and out on screen
public class UIFadeController : MonoBehaviour
{
    [Tooltip("How fast the UI fades in and out")]
    public float fadeSpeed = 3f;
    [Tooltip("How long the UI stays before fading out again")]
    public float idleDelay = 2f;

    public static UIFadeController Instance;

    // List of scenes where the UI does not fade out
    public SceneReference[] excludedScenes;

    private CanvasGroup canvasGroup;    // Controls UI visability
    private float lastActiveTime;       // Tracks last time UI was used
    private bool isActive = true;       // Whether UI should currently be visable or not
    [HideInInspector] public bool inExcludedScene = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Get canvasgroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup missing on UIFadeController!");
        }

        // Initialize last ative time to now
        lastActiveTime = Time.time;
    }

    private void OnEnable()
    {
        // Subscribe to scene load event to check if fading should be disabled
        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
        }
    }

    private void OnSceneLoaded()
    {
        // Check if current scene is in excluded list
        foreach (SceneReference scene in excludedScenes)
        {
            if (SceneManager.GetActiveScene().name == scene.GetSceneName())
            {
                // Disable fading if true
                inExcludedScene = true;
                return;
            }
        }
        
        // Continue as normal if not in excluded scene
        Debug.Log("UIFadeController: Current scene is not in the excluded list, UI will fade as normal.");

        // Reset flag if player was previously in an excluded scene
        if (inExcludedScene)
        {
            inExcludedScene = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Scenes where the UI will not fade away
        if (inExcludedScene)
        {
            if (canvasGroup.alpha < 1.0f)
            {
                canvasGroup.alpha = 1.0f;
            }
            return;
        }
        
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

    private void OnDisable()
    {
        if (SceneLoadManager.Instance != null) SceneLoadManager.Instance.OnSceneLoaded.RemoveListener(OnSceneLoaded);
    }
}
