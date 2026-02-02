using System;
using System.Collections.Generic;

[Serializable]
public class EnemyNPCListSaveData
{
    public List<EnemyNPCSaveData> enemyNPCs = new List<EnemyNPCSaveData>();
}

[Serializable]
public class EnemyNPCSaveData
{
    public string id;
    public float[] position;
    public float[] rotation;
    public bool isChasingNPC;
    public bool reachedNPC;
    public bool isChasingPlayer;
    public bool isDistracted;
    public bool isPossessed;
    public bool isActive;
}
