using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    private SerializedProperty itemNameProp;
    private SerializedProperty itemTypeProp;
    private SerializedProperty invIconProp;
    private SerializedProperty worldPrefabProp;
    private SerializedProperty visualPrefabProp;
    private SerializedProperty equippedScaleProp;
    private SerializedProperty equippedPosProp;
    private SerializedProperty equippedRotProp;
    private SerializedProperty descriptionProp;

    private void OnEnable()
    {
        itemNameProp = serializedObject.FindProperty("itemName");
        itemTypeProp = serializedObject.FindProperty("itemType");
        invIconProp = serializedObject.FindProperty("invIcon");
        worldPrefabProp = serializedObject.FindProperty("worldPrefab");
        visualPrefabProp = serializedObject.FindProperty("visualPrefab");
        equippedScaleProp = serializedObject.FindProperty("equippedScaleTransform");
        equippedPosProp = serializedObject.FindProperty("equippedPositionOffset");
        equippedRotProp = serializedObject.FindProperty("equippedRotationOffset");
        descriptionProp = serializedObject.FindProperty("description");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw main fields in original order
        EditorGUILayout.PropertyField(itemNameProp);
        EditorGUILayout.PropertyField(itemTypeProp);
        EditorGUILayout.PropertyField(invIconProp);
        EditorGUILayout.PropertyField(worldPrefabProp);
        EditorGUILayout.PropertyField(visualPrefabProp);

        // Transform Settings header (only show contents for certain types)
        EditorGUILayout.Space();

        var itemType = (ItemType)itemTypeProp.enumValueIndex;
        if (itemType == ItemType.ThrowableItem || itemType == ItemType.GrabbableItem)
        {
            EditorGUILayout.LabelField("Transform Settings (adjusts the transforms of the visual object)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(equippedScaleProp, new GUIContent("Equipped Scale Transform"));
            EditorGUILayout.PropertyField(equippedPosProp, new GUIContent("Equipped Position Offset"));
            EditorGUILayout.PropertyField(equippedRotProp, new GUIContent("Equipped Rotation Offset"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);

        var textAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            richText = true
        };

        descriptionProp.stringValue = EditorGUILayout.TextArea(descriptionProp.stringValue, textAreaStyle, GUILayout.MinHeight(60));

        serializedObject.ApplyModifiedProperties();
    }
}
