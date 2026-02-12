using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonIcons : MonoBehaviour
{
    public static ButtonIcons Instance;

    [System.Serializable]
    public class InteractionIcon
    {
        public InteractType type;
        public RawImage icon;
    }

    [SerializeField] private List<InteractionIcon> icons;

    private Dictionary<InteractType, RawImage> iconMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        iconMap = new Dictionary<InteractType, RawImage>();

        foreach (var entry in icons)
        {
            if (entry.icon != null)
            {
                iconMap[entry.type] = entry.icon;
                entry.icon.gameObject.SetActive(false);
            }
        }
    }

    public void Highlight (InteractType type)
    {
        //Clear();

        if (iconMap.TryGetValue(type, out RawImage icon))
        {
            if (icon != null)
            {
                icon.gameObject.SetActive(true);
            }   
        }
    }

    public void Clear()
    {
        foreach (var icon in iconMap.Values)
        {
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
