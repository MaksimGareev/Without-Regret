using System.Collections.Generic;
using UnityEngine;

public class InteractionTutorialManager : MonoBehaviour
{
    public static InteractionTutorialManager Instance;

    private HashSet<InteractType> shownTutorials = new HashSet<InteractType>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasSeenTutorial(InteractType type)
    {
        return shownTutorials.Contains(type);
    }

    public void MarkTutorialSeen(InteractType type)
    {
        if (!shownTutorials.Contains(type))
            shownTutorials.Add(type);
    }
}
