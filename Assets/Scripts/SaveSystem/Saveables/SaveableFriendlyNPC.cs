using UnityEngine;

public class SaveableFriendlyNPC : SaveableWithID
{
    public override void SaveTo(SaveData data)
    {
        FriendlyNPCSaveData state = new FriendlyNPCSaveData();

        state.id = GetUniqueID();
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
            Debug.Log($"Saving Barry: isTraveling={state.isTraveling}, arrived={state.arrived}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<Darry>() != null)
        {
            Darry darry = GetComponent<Darry>();
            state.isTraveling = darry.isTraveling;
            state.arrived = darry.arrived;
            // Darry does not have a canFollowPlayer property
            // Darry does not have an isFollowingPlayer property
            Debug.Log($"Saving Darry: isTraveling={state.isTraveling}, arrived={state.arrived}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<DarryNeighborhood>() != null)
        {
            DarryNeighborhood darry = GetComponent<DarryNeighborhood>();
            state.isTraveling = darry.isTraveling;
            state.arrived = darry.arrived;
            // DarryNeighborhood does not have a canFollowPlayer property
            // DarryNeighborhood does not have an isFollowingPlayer property
            Debug.Log($"Saving DarryNeighborhood: isTraveling={state.isTraveling}, arrived={state.arrived}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<Irene>() != null)
        {
            Irene irene = GetComponent<Irene>();
            state.isTraveling = irene.isTraveling;
            state.arrived = irene.arrived;
            state.canFollowPlayer = irene.CanFollowPlayer;
            state.isFollowingPlayer = irene.IsFollowing;
            Debug.Log($"Saving Irene: canFollowPlayer={state.canFollowPlayer}, isFollowingPlayer={state.isFollowingPlayer}, ID: {GetUniqueID()}");
        }
        else
        {
            Debug.LogWarning("SaveableFriendlyNPC attached to an unknown NPC type.");
        }

        if (GetComponent<DialogueTrigger>())
        {
            DialogueTrigger dialogueTrigger = GetComponent<DialogueTrigger>();
            state.isLookingAtPlayer = dialogueTrigger.isLookingAtPlayer;
            state.talkedAlready = dialogueTrigger.TalkedAlready;
            Debug.Log($"Saving DialogueTrigger: isLookingAtPlayer={state.isLookingAtPlayer}, talkedAlready={state.talkedAlready}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<NewDialogueTrigger>())
        {
            NewDialogueTrigger newDialogueTrigger = GetComponent<NewDialogueTrigger>();
            state.isLookingAtPlayer = newDialogueTrigger.isLookingAtPlayer;
            state.talkedAlready = newDialogueTrigger.hasTalked;
            Debug.Log($"Saving NewDialogueTrigger: isLookingAtPlayer={state.isLookingAtPlayer}, talkedAlready={state.talkedAlready}, ID: {GetUniqueID()}");
        }

        if (data.friendlyNPCListSaveData.friendlyNPCs.Exists(npc => npc.id == GetUniqueID()))
        {
            data.friendlyNPCListSaveData.friendlyNPCs.RemoveAll(npc => npc.id == GetUniqueID());
        }

        data.friendlyNPCListSaveData.friendlyNPCs.Add(state);
    }

    public override void LoadFrom(SaveData data)
    {
        var state = data.friendlyNPCListSaveData.friendlyNPCs.Find(npc => npc.id == GetUniqueID());

        if (state == null)
        {
            Debug.LogWarning($"Loading Failed: No saved state found for Friendly NPC with ID: {GetUniqueID()}");
            return;
        }
        else
        {
            Debug.Log($"Loading Friendly NPC with ID: {GetUniqueID()}");
        }

        transform.position = new Vector3(state.position[0], state.position[1], state.position[2]);
        transform.eulerAngles = new Vector3(state.rotation[0], state.rotation[1], state.rotation[2]);
        gameObject.SetActive(state.isActive);

        if (GetComponent<Barry>() != null)
        {
            Barry barry = GetComponent<Barry>();
            barry.isTraveling = state.isTraveling;
            barry.arrived = state.arrived;
            Debug.Log($"Loading Barry: isTraveling={state.isTraveling}, arrived={state.arrived}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<Darry>() != null)
        {
            Darry darry = GetComponent<Darry>();
            darry.isTraveling = state.isTraveling;
            darry.arrived = state.arrived;
            Debug.Log($"Loading Darry: isTraveling={state.isTraveling}, arrived={state.arrived}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<DarryNeighborhood>() != null)
        {
            DarryNeighborhood darry = GetComponent<DarryNeighborhood>();
            darry.isTraveling = state.isTraveling;
            darry.arrived = state.arrived;
            Debug.Log($"Loading DarryNeighborhood: isTraveling={state.isTraveling}, arrived={state.arrived}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<Irene>() != null)
        {
            Irene irene = GetComponent<Irene>();
            irene.isTraveling = state.isTraveling;
            irene.arrived = state.arrived;
            irene.CanFollowPlayer = state.canFollowPlayer;
            irene.IsFollowing = state.isFollowingPlayer;
            Debug.Log($"Loading Irene: canFollowPlayer={state.canFollowPlayer}, isFollowingPlayer={state.isFollowingPlayer}, ID: {GetUniqueID()}");
        }
        else if (GetComponent<DialogueTrigger>() == null)
        {
            Debug.LogWarning("SaveableFriendlyNPC attached to an unknown NPC type.");
        }

        if (GetComponent<DialogueTrigger>() != null)
        {
            DialogueTrigger dialogueTrigger = GetComponent<DialogueTrigger>();
            dialogueTrigger.isLookingAtPlayer = state.isLookingAtPlayer;
            dialogueTrigger.TalkedAlready = state.talkedAlready;
            Debug.Log($"Loading DialogueTrigger: isLookingAtPlayer={state.isLookingAtPlayer}, talkedAlready={state.talkedAlready}, ID: {GetUniqueID()}");
        }
    }
}
