using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SwarmState))]
public class SwarmStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Reset to starting swarm"))
        {
            var state = (SwarmState)target;
            state.Reset();
            EditorUtility.SetDirty(state);
        }
    }

}
