using System;
using System.Collections.Generic;

[Serializable]
public class PlayerTransformEntry
{
    public string sceneName;
    public float[] position;
    public float[] rotation;
}

[Serializable]
public class PlayerSaveData
{
    public List<PlayerTransformEntry> playerTransforms = new List<PlayerTransformEntry>();
    public TimerRingUI.RingState currentRingState = TimerRingUI.RingState.Full;

    public void SetPlayerTransform(string scene, float[] pos, float[] rot)
    {
        var entry = playerTransforms.Find(e => e.sceneName == scene);
        if (entry == null)
        {
            entry = new PlayerTransformEntry
            {
                sceneName = scene,
                position = pos,
                rotation = rot
            };
            playerTransforms.Add(entry);
        }
        else
        {
            entry.position = pos;
            entry.rotation = rot;
        }
    }

    public bool TryGetPlayerTransform(string scene, out float[] position, out float[] rotation)
    {
        var entry = playerTransforms.Find(e => e.sceneName == scene);
        if (entry != null)
        {
            position = entry.position;
            rotation = entry.rotation;
            return true;
        }
        
        position = null;
        rotation = null;
        return false;
    }
}
