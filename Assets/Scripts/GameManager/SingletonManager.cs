using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public enum SingletonType
{
    EventSystem,
    MainCanvas,
    PlayerUICanvas,
    interactionIconsCanvas,
    DialogueManager,
}

public class SingletonManager : MonoBehaviour
{
    public static Dictionary<SingletonType, SingletonManager> instances = new Dictionary<SingletonType, SingletonManager>();
    public SingletonType singletonType;

    private void Awake()
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(SingletonType)).Length; i++)
        {
            if (!instances.ContainsKey((SingletonType)i) && this.singletonType == (SingletonType)i)
            {
                instances.Add(singletonType, this);
                DontDestroyOnLoad(this.gameObject);
            }
            else if (instances.ContainsKey((SingletonType)i) && this.singletonType == (SingletonType)i)
            {
                Destroy(this.gameObject);
            }
        }

        CheckForOtherSingletons();
    }

    private void Start()
    {
        CheckForOtherSingletons();
    }

    private void CheckForOtherSingletons()
    {
        // Delete other event system instances
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (var eventSystem in eventSystems)
        {
            if (instances.ContainsKey(SingletonType.EventSystem))
            {
                if (eventSystem.gameObject != instances[SingletonType.EventSystem].gameObject)
                {
                    Destroy(eventSystem.gameObject);
                }
            }
        }
    }
}
