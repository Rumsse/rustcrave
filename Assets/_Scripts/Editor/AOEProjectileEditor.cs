using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AOEProjectile))]
public class AOEProjectileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var animation = serializedObject.FindProperty("_animation").objectReferenceValue;
        var effect = serializedObject.FindProperty("_effect").objectReferenceValue;
        
        if(!animation && !effect)
            EditorGUILayout.HelpBox("At least one of Animation and Effect should be set", MessageType.Warning);
        
        base.OnInspectorGUI();
    }
}
