using UnityEngine;

public class SaveableFriendlyNPC : MonoBehaviour, ISaveable
{
    private string uniqueID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
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
        FriendlyNPCSaveData state = new FriendlyNPCSaveData();

        state.id = uniqueID;
        state.position = new float[] { transform.position.x, transform.position.y, transform.position.z };
        state.rotation = new float[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        state.isActive = gameObject.activeSelf;

        if (GetComponent<Barry>() != null)
        {
            Barry barry = GetComponent<Barry>();
            state.isTraveling = barry.isTraveling;
            state.arrived = barry.arrived;
            // Barry does not have a canFollowPlayer property
            // Barry does not have an isFollowingPlayer property
        }
        else if (GetComponent<Darry>() != null)
        {
            Darry darry = GetComponent<Darry>();
            state.isTraveling = darry.isTraveling;
            state.arrived = darry.arrived;
            // Darry does not have a canFollowPlayer property
            // Darry does not have an isFollowingPlayer property
        }
        else if (GetComponent<DarryNeighborhood>() != null)
        {
            DarryNeighborhood darry = GetComponent<DarryNeighborhood>();
            state.isTraveling = darry.isTraveling;
            state.arrived = darry.arrived;
            // DarryNeighborhood does not have a canFollowPlayer property
            // DarryNeighborhood does not have an isFollowingPlayer property
        }
        else if (GetComponent<Irene>() != null)
        {
            Irene irene = GetComponent<Irene>();
            state.isTraveling = irene.isTraveling;
            state.arrived = irene.arrived;
            state.canFollowPlayer = irene.CanFollowPlayer;
            state.isFollowingPlayer = irene.IsFollowing;
        }
        else
        {
            Debug.LogWarning("SaveableFriendlyNPC attached to an unknown NPC type.");
        }

        if (GetComponent<DialogueTrigger>() != null)
        {
            DialogueTrigger dialogueTrigger = GetComponent<DialogueTrigger>();
            state.isLookingAtPlayer = dialogueTrigger.isLookingAtPlayer;
            state.talkedAlready = dialogueTrigger.TalkedAlready;
        }

        if (data.friendlyNPCListSaveData.friendlyNPCs.Exists(npc => npc.id == uniqueID))
        {
            data.friendlyNPCListSaveData.friendlyNPCs.RemoveAll(npc => npc.id == uniqueID);
        }

        data.friendlyNPCListSaveData.friendlyNPCs.Add(state);
    }

    public void LoadFrom(SaveData data)
    {
        var state = data.friendlyNPCListSaveData.friendlyNPCs.Find(npc => npc.id == uniqueID);

        if (state == null)
        {
            Debug.LogWarning("No saved state found for Friendly NPC with ID: " + uniqueID);
            return;
        }

        transform.position = new Vector3(state.position[0], state.position[1], state.position[2]);
        transform.eulerAngles = new Vector3(state.rotation[0], state.rotation[1], state.rotation[2]);
        gameObject.SetActive(state.isActive);

        if (GetComponent<Barry>() != null)
        {
            Barry barry = GetComponent<Barry>();
            barry.isTraveling = state.isTraveling;
            barry.arrived = state.arrived;
        }
        else if (GetComponent<Darry>() != null)
        {
            Darry darry = GetComponent<Darry>();
            darry.isTraveling = state.isTraveling;
            darry.arrived = state.arrived;
        }
        else if (GetComponent<DarryNeighborhood>() != null)
        {
            DarryNeighborhood darry = GetComponent<DarryNeighborhood>();
            darry.isTraveling = state.isTraveling;
            darry.arrived = state.arrived;
        }
        else if (GetComponent<Irene>() != null)
        {
            Irene irene = GetComponent<Irene>();
            irene.isTraveling = state.isTraveling;
            irene.arrived = state.arrived;
            irene.CanFollowPlayer = state.canFollowPlayer;
            irene.IsFollowing = state.isFollowingPlayer;
        }
        else
        {
            Debug.LogWarning("SaveableFriendlyNPC attached to an unknown NPC type.");
        }

        if (GetComponent<DialogueTrigger>() != null)
        {
            DialogueTrigger dialogueTrigger = GetComponent<DialogueTrigger>();
            dialogueTrigger.isLookingAtPlayer = state.isLookingAtPlayer;
            dialogueTrigger.TalkedAlready = state.talkedAlready;
        }
    }
}
