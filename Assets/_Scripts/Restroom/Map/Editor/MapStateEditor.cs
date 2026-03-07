using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapState))]
public class MapStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var mapState = (MapState)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Reset Map"))
        {
            mapState.currentRow = -1;
            mapState.currentColumn = 1;
            mapState.nodes.Clear();
            EditorUtility.SetDirty(mapState);
            Debug.Log("Map reset!");
        }

        if (GUILayout.Button("Regenerate Map"))
        {
            mapState.Initialize();
            EditorUtility.SetDirty(mapState);
            Debug.Log($"Map regenerated! {mapState.nodes.Count} nodes created.");
        }

        if (Application.isPlaying)
            return;

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            $"Current: row {mapState.currentRow}, col {mapState.currentColumn}\nNodes: {mapState.nodes.Count}",
            MessageType.Info
        );
    }
}