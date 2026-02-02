using UnityEngine;

public class SaveableEnemyNPC : MonoBehaviour, ISaveable
{
    private string uniqueID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID) && this.gameObject.activeSelf)
        {
            uniqueID = System.Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }

    public string GetUniqueID() => uniqueID;

    public void SaveTo(SaveData data)
    {
        EnemyNPCSaveData state = new EnemyNPCSaveData();
        state.id = uniqueID;
        state.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        state.rotation = new float[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        state.isActive = gameObject.activeSelf;

        if (GetComponent<ChasingEnemy>() != null)
        {
            ChasingEnemy chasing = GetComponent<ChasingEnemy>();
            state.currentTarget = chasing.currentTarget;
        }
        else if (GetComponent<EnemyDistracted>() != null)
        {
            EnemyDistracted distracted = GetComponent<EnemyDistracted>();
            state.isDistracted = distracted.isDistracted;
        }
        else
        {
            Debug.LogWarning("SaveableEnemyNPC attached to an unknown Enemy type.");
        }

        if (data.enemyNPCListSaveData.enemyNPCs.Exists(enemy => enemy.id == uniqueID))
        {
            data.enemyNPCListSaveData.enemyNPCs.RemoveAll(enemy => enemy.id == uniqueID);
        }

        data.enemyNPCListSaveData.enemyNPCs.Add(state);
    }

    public void LoadFrom(SaveData data)
    {
        var state = data.enemyNPCListSaveData.enemyNPCs.Find(enemy => enemy.id == uniqueID);
        
        if (state == null)
        {
            Debug.LogWarning($"No save data found for Enemy NPC with ID: {uniqueID}");
            return;
        }

        transform.position = new Vector3(state.position[0], state.position[1], state.position[2]);
        transform.eulerAngles = new Vector3(state.rotation[0], state.rotation[1], state.rotation[2]);
        gameObject.SetActive(state.isActive);

        if (GetComponent<ChasingEnemy>() != null)
        {
            ChasingEnemy chasing = GetComponent<ChasingEnemy>();
            chasing.currentTarget = state.currentTarget;
        }
        else if (GetComponent<EnemyDistracted>() != null)
        {
            EnemyDistracted distracted = GetComponent<EnemyDistracted>();
            distracted.isDistracted = state.isDistracted;
        }
        else
        {
            Debug.LogWarning("SaveableEnemyNPC attached to an unknown Enemy type.");
        }
    }
}
