using UnityEngine;

public class SaveableBossEnemy : SaveableWithID
{
    public override void SaveTo(SaveData data)
    {
        var boss = GetComponent<BossEnemyController>();
        
        if (boss)
        {
            BossEnemySaveData state = new BossEnemySaveData();
            state.id = GetUniqueID();
            state.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
            state.rotation = new float[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
            state.isActive = gameObject.activeSelf;
            state.currentPhase = boss.GetCurrentPhase();

            data.bossEnemySaveData = state;
        }
        else
        {
            Debug.LogWarning($"SaveableBossEnemy attached to " + gameObject.name + " which does not have a BossEnemyController component. Cannot save any data.");
        }
    }

    public override void LoadFrom(SaveData data)
    {
        var boss = GetComponent<BossEnemyController>();
        var state = data?.bossEnemySaveData;

        if (state == null)
        {
            Debug.LogWarning($"Loading Failed: No save data found for Boss Enemy with ID: {GetUniqueID()}");
            return;
        }
        
        if (!boss)
        {
            Debug.LogWarning($"Loading Failed: SaveableBossEnemy attached to " + gameObject.name + " which does not have a BossEnemyController component. Cannot load any data.");
            return;
        }
        
        Debug.Log($"Loading Boss Enemy with ID: {GetUniqueID()}");

        transform.position = new Vector3(state.position[0], state.position[1], state.position[2]);
        transform.eulerAngles = new Vector3(state.rotation[0], state.rotation[1], data.bossEnemySaveData.rotation[2]);
        gameObject.SetActive(state.isActive);

        if (state.isActive) 
        {
            boss.LoadIntoPhase(state.currentPhase);
        }
    }
}
