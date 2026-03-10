using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActiveModifier))]
public class ActiveModifierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var active = (ActiveModifier)target;

        EditorGUILayout.Space(10);

        var current = active.current != null ? active.current.DisplayName : "None";
        EditorGUILayout.LabelField("Current Modifier", current);

        if (active.current != null)
        {
            EditorGUILayout.LabelField("Enemy Multiplier", $"x{active.EnemySpawnMultiplier}");
            EditorGUILayout.LabelField("Resource Multiplier", $"x{active.ResourceSpawnMultiplier}");
        }

        if (GUILayout.Button("Clear Modifier"))
        {
            active.Clear();
            EditorUtility.SetDirty(active);
            Debug.Log("Active modifier cleared!");
        }
    }
}