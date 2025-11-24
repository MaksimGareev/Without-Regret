using System.Collections.Generic;

[System.Serializable]
public class InventorySaveData
{
    public List<ItemData> items = new List<ItemData>();
    public List<ItemData> keyItems = new List<ItemData>();
    public List<ItemData> otherItems = new List<ItemData>();
    public bool hasBackpack;
}
