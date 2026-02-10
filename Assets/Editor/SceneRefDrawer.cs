#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneRefDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var sceneAssetProp = property.FindPropertyRelative("sceneAsset");
        var scenePathProp = property.FindPropertyRelative("scenePath");

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        
        EditorGUI.PropertyField(position, sceneAssetProp, label);

        if (EditorGUI.EndChangeCheck())
        {
            var sceneAsset = sceneAssetProp.objectReferenceValue;
            if (sceneAsset != null)
            {
                scenePathProp.stringValue = AssetDatabase.GetAssetPath(sceneAsset);
            }
            else
            {
                scenePathProp.stringValue = string.Empty;
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }
}
#endif
