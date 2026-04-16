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

        if (GUILayout.Button("Reset to Start"))
        {
            mapState.scannedNodes.Clear();
            mapState.visitedNodes.Clear();
            mapState.currentNodeId = string.Empty;

            EditorUtility.SetDirty(mapState);
        }

        if (GUILayout.Button("Regenerate Map"))
        {
            mapState.Initialize();
            EditorUtility.SetDirty(mapState);
        }

        if (Application.isPlaying)
            return;

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            $"Current Node Row: {mapState.currentRow}\nVisited Nodes: {mapState.visitedNodes.Count}\nTotal Generated Nodes: {mapState.nodes.Count}",
            MessageType.Info
        );
    }
}