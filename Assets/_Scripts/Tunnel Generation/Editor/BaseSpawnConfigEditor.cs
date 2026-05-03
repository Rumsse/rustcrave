using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseSpawnConfig), true)]
public class BaseSpawnConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Runtime Value (Runtime Debug)", EditorStyles.boldLabel);

        BaseSpawnConfig config = (BaseSpawnConfig)target;

        if (config.Entries == null || config.Entries.Count == 0)
        {
            EditorGUILayout.HelpBox("Add elements to the list to see a preview.", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        foreach (var entry in config.Entries)
        {
            if (entry == null || entry.Prefab == null) continue;

            float baseChance = entry.SpawnChance;
            float currentChance = config.GetModifiedChance(entry);

            string nameText = $"{entry.Prefab.name} [{entry.SpawnTag}]";

            if (Mathf.Approximately(baseChance, currentChance))
            {
                EditorGUILayout.LabelField(nameText, $"{baseChance:F2}");
            }
            else
            {
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.normal.textColor = Color.green;
                EditorGUILayout.LabelField(nameText, $"{baseChance:F2}  ?  {currentChance:F2}", style);
            }
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Total chance:", $"{config.TotalChance:F2}", EditorStyles.boldLabel);

        EditorGUILayout.EndVertical();

        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}