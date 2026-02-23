using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ButtonIcons : MonoBehaviour
{
    public static ButtonIcons Instance;

    [System.Serializable]
    public class InteractionIcon
    {
        public InteractType type;
        public RawImage icon;
        public TextMeshProUGUI text;
    }

    [SerializeField] private List<InteractionIcon> icons;

    private Dictionary<InteractType, InteractionIcon> iconMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        iconMap = new Dictionary<InteractType, InteractionIcon>();

        foreach (var entry in icons)
        {
            if (!iconMap.ContainsKey(entry.type))
            {
                iconMap.Add(entry.type, entry);
            }

            if (entry.icon != null)
            {
                entry.icon.gameObject.SetActive(false);
            }
            
            if (entry.text != null)
            {
                entry.text.gameObject.SetActive(false);
            }
        }
    }

    public void Highlight(InteractType type)
    {
        //Clear();

        if (iconMap.TryGetValue(type, out InteractionIcon entry))
        {
            if (entry.icon != null)
            {
                entry.icon.gameObject.SetActive(true);
            }
            
            if (entry.text != null)
            {
                entry.text.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning($"No icon found for InteractType {type}");
        }
    }

    public void HighlightMultiple(List<IInteractable> interactables)
    {
        //Clear();

        foreach (var interactable in interactables)
        {
            Highlight(interactable.interactType);
        }
    }

    public void Clear()
    {
        foreach (var entry in iconMap.Values)
        {
            if (entry.icon != null)
            {
                entry.icon.gameObject.SetActive(false);
            }
            if (entry.text != null)
            {
                entry.text.gameObject.SetActive(false);
            }
        }
    }

}