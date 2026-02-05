using System;
using System.Collections.Generic;

[Serializable]
public class FriendlyNPCListSaveData
{
    public List<FriendlyNPCSaveData> friendlyNPCs = new List<FriendlyNPCSaveData>();
}

[Serializable]
public class FriendlyNPCSaveData
{
    public string id;
    public float[] position;
    public float[] rotation;
    public bool isTraveling;
    public bool arrived;
    public bool canFollowPlayer;
    public bool isFollowingPlayer;
    public bool isLookingAtPlayer;
    public bool talkedAlready;
    public bool isActive;
}
