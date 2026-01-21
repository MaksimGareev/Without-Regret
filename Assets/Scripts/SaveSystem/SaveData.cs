[System.Serializable]
public class SaveData
{
    public string lastSceneName;
    
    public PlayerSaveData playerSaveData;
    public ObjectiveSaveData objectiveSaveData;
    public WorldSaveData worldSaveData;
    public InventorySaveData inventorySaveData;
    public int slot = 1;

    public SaveData(int slot)
    {
        this.slot = slot;
        playerSaveData = new PlayerSaveData();
        objectiveSaveData = new ObjectiveSaveData();
        worldSaveData = new WorldSaveData();
        inventorySaveData = new InventorySaveData();
    }
}
