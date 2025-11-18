using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string currentSceneName;
    public int currentSceneIndex;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    // Know what scene the player is in
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log("Scene changed to: " + newScene.name);
        currentSceneName = newScene.name;
        currentSceneIndex = newScene.buildIndex;
    }
}
