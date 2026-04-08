using System;

[Serializable]
public class BossEnemySaveData
{
    public string id;
    public float[] position;
    public float[] rotation;
    public bool isActive;
    public int currentPhase;
}
