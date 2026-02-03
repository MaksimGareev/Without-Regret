using UnityEngine;

public class SettingsBootstrap : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameSettings.LoadFromPrefs();
    }
}
