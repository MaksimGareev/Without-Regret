using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemSingleton : MonoBehaviour
{
    public static EventSystemSingleton instance;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of EventSystemSingleton exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }

        CheckForOtherEventSystems();
    }

    private void Start()
    {
        CheckForOtherEventSystems();
    }

    private void CheckForOtherEventSystems()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (var eventSystem in eventSystems)
        {
            if (eventSystem.gameObject != instance.gameObject)
            {
                Destroy(eventSystem.gameObject);
            }
        }
    }
}
