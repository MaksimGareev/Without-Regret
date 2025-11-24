[System.Serializable]
public class SaveData
{
    public string lastSceneName;
    
    public PlayerSaveData playerSaveData = new PlayerSaveData();
    public ObjectiveSaveData objectiveSaveData = new ObjectiveSaveData();
    public WorldSaveData worldSaveData = new WorldSaveData();
    public InventorySaveData inventorySaveData = new InventorySaveData();

    public int version = 1;
}
