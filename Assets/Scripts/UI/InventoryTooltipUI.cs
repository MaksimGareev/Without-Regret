using TMPro;
using UnityEngine;

public class InventoryTooltipUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform backgroundTransform;

    private void Awake()
    {
        Hide();
    }

    public void Show(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        itemNameText.text = item.ItemName;
        descriptionText.text = item.Description;
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
