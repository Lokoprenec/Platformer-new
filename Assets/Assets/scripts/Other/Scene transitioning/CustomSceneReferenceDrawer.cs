#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [SceneName] with string.");
            return;
        }

        var scenes = EditorBuildSettings.scenes;
        string[] sceneNames = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
        }

        int index = Mathf.Max(0, System.Array.IndexOf(sceneNames, property.stringValue));
        index = EditorGUI.Popup(position, label.text, index, sceneNames);
        property.stringValue = sceneNames[index];
    }
}
#endif

