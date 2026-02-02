using System;
using System.Collections.Generic;
[Serializable]
public class GateListSaveData
{
    public List<GateSaveData> gates = new List<GateSaveData>();
}

[Serializable]
public class GateSaveData
{
    public string id;
    public bool locked;
    public bool opened;
}
