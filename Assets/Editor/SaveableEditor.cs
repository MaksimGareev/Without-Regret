#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveableWithID), true)]
public class SaveableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var saveableID = (SaveableWithID)target;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("SaveSystem UniqueID", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("Unique ID", saveableID.GetUniqueID());
        }

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Refresh Unique ID"))
        {
            Undo.RecordObject(saveableID, "Refresh Unique ID");
            saveableID.RefreshUniqueID();
            EditorUtility.SetDirty(saveableID);
        }

        EditorGUILayout.HelpBox("The Unique ID is used by the Save System to identify this object uniquely in the save data. Refreshing it will generate a new ID, which will break existing save files that reference this NPC. Use with caution. Delete existing save files after use.", MessageType.Warning);
    }
}
#endif