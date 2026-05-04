using UnityEngine;
using UnityEditor;
using TMPro;

public class GlobalSpacingChanger : EditorWindow
{
    #region Setup

    private float characterSpacing = 7f;
    private float wordSpacing = 10f;
    private float lineSpacing = 20f;
    private float paragraphSpacing = 0f;

    [MenuItem("Tools/Change Global Spacing")]
    public static void ShowWindow() => GetWindow<GlobalSpacingChanger>("Spacing Changer");

    private void OnGUI()
    {
        characterSpacing = EditorGUILayout.FloatField("Character Spacing", characterSpacing);
        wordSpacing = EditorGUILayout.FloatField("Word Spacing", wordSpacing);
        lineSpacing = EditorGUILayout.FloatField("Line Spacing", lineSpacing);
        paragraphSpacing = EditorGUILayout.FloatField("Paragraph Spacing", paragraphSpacing);

        if (GUILayout.Button("Apply Spacing to All TextMeshPro"))
            ApplySpacing();
    }
    #endregion

    #region Logic

    private void ApplySpacing()
    {
        ChangeInLoadedScenes();
        ChangeInPrefabs();

        AssetDatabase.SaveAssets();
        Debug.Log("Spacing replacement completed.");
    }

    private void ChangeInLoadedScenes()
    {
        var allTextComponents = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var text in allTextComponents)
            ApplySettingsToText(text, text);
    }

    private void ChangeInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                continue;

            var textComponents = prefab.GetComponentsInChildren<TMP_Text>(true);

            if (textComponents.Length == 0)
                continue;

            foreach (var text in textComponents)
                ApplySettingsToText(text, prefab);
        }
    }

    private void ApplySettingsToText(TMP_Text text, Object dirtyTarget)
    {
        if (text.characterSpacing == characterSpacing && text.wordSpacing == wordSpacing && text.lineSpacing == lineSpacing && text.paragraphSpacing == paragraphSpacing)
            return;

        Undo.RecordObject(text, "Change Spacing");
        text.characterSpacing = characterSpacing;
        text.wordSpacing = wordSpacing;
        text.lineSpacing = lineSpacing;
        text.paragraphSpacing = paragraphSpacing;
        EditorUtility.SetDirty(dirtyTarget);
    }
    #endregion
}