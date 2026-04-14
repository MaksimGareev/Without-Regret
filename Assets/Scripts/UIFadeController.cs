using UnityEngine;
using UnityEngine.SceneManagement;

public class UIFadeController : MonoBehaviour
{
    public float fadeSpeed = 3f;
    public float idleDelay = 2f;

    public static UIFadeController Instance;

    public SceneReference[] excludedScenes;

    private CanvasGroup canvasGroup;
    private float lastActiveTime;
    private bool isActive = true;
    [HideInInspector] public bool inExcludedScene = false;

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

    private void OnEnable()
    {
        if (SceneLoadManager.Instance)
        {
            SceneLoadManager.Instance.OnSceneLoaded.AddListener(OnSceneLoaded);
        }
    }

    private void OnSceneLoaded()
    {
        foreach (SceneReference scene in excludedScenes)
        {
            if (SceneManager.GetActiveScene().name == scene.GetSceneName())
            {
                inExcludedScene = true;
                return;
            }
        }
        
        Debug.Log("UIFadeController: Current scene is not in the excluded list, UI will fade as normal.");

        if (inExcludedScene)
        {
            inExcludedScene = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
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
