using System;
using System.Collections.Generic;
using UnityEngine;

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
    public Transform currentTarget;
    public bool isDistracted;
    public bool isActive;
}
