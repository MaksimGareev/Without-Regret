#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[System.Serializable]
public class SceneReference
{
#if UNITY_EDITOR
    [Tooltip("The scene to load when the door is interacted with. Plug the scene from the project folder into here.")]
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;

    public void EditorSyncPathFromAsset()
    {
        if (Application.isPlaying) return;

        if (sceneAsset != null)
        {
            scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
        }
        else
        {
            scenePath = string.Empty;
        }
    }
#endif
    [SerializeField, HideInInspector] private string scenePath;
    private string sceneName => 
        string.IsNullOrEmpty(scenePath) ? string.Empty : System.IO.Path.GetFileNameWithoutExtension(scenePath);

    public string GetSceneName() => sceneName;
}