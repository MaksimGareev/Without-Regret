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

        if (boss)
        {
            transform.position = new Vector3(data.bossEnemySaveData.position[0], data.bossEnemySaveData.position[1], data.bossEnemySaveData.position[2]);
            transform.eulerAngles = new Vector3(data.bossEnemySaveData.rotation[0], data.bossEnemySaveData.rotation[1], data.bossEnemySaveData.rotation[2]);
            gameObject.SetActive(data.bossEnemySaveData.isActive);
            boss.LoadIntoPhase(data.bossEnemySaveData.currentPhase);
        }
        else
        {
            Debug.LogWarning($"SaveableBossEnemy attached to " + gameObject.name + " which does not have a BossEnemyController component. Cannot load any data.");
        }
    }
}
