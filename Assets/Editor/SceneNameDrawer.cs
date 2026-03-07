using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path))
            .ToArray();

        if (scenes.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "No scenes in Build Settings");
            return;
        }

        int currentIndex = Mathf.Max(0, System.Array.IndexOf(scenes, property.stringValue));

        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, scenes);

        property.stringValue = scenes[currentIndex];
    }
}