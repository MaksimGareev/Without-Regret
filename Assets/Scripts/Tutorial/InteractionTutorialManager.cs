using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTutorialManager : MonoBehaviour, ISaveable
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
        
        RegisterAsSaveable();
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

    public void SaveTo(SaveData data)
    {
        List<InteractType> tutorialList = new List<InteractType>();
        
        foreach (InteractType type in shownTutorials)
        {
            tutorialList.Add(type);
        }
        
        data.shownTutorials = tutorialList;
    }

    public void LoadFrom(SaveData data)
    {
        HashSet<InteractType> tutorialHashSet = new HashSet<InteractType>();

        foreach (InteractType type in data.shownTutorials)
        {
            tutorialHashSet.Add(type);
        }
        
        shownTutorials = tutorialHashSet;
    }
    
    private void RegisterAsSaveable()
    {
        // Register self with SaveManager as a savable entity
        if (SaveManager.Instance)
        {
            SaveManager.Instance.RegisterSaveable(this);
        }
        else
        {
            StartCoroutine(RegisterWhenReady());
        }
    }
    
    // Wait until SaveManager instance is available before registering, since SaveManager is 
    // also a singleton and may not be initialized yet when ObjectiveManager's Awake is called.
    private IEnumerator RegisterWhenReady()
    {
        while (!SaveManager.Instance)
        {
            yield return null;
        }

        SaveManager.Instance.RegisterSaveable(this);
        Debug.Log("DialogueManager Registered with SaveManager");
    }
}
