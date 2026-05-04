using UnityEngine;
using UnityEditor;
using TMPro;

public class GlobalFontChanger : EditorWindow
{
    #region Setup

    private TMP_FontAsset targetFont;

    [MenuItem("Tools/Change Global Font")]
    public static void ShowWindow() => GetWindow<GlobalFontChanger>("Font Changer");

    private void OnGUI()
    {
        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font", targetFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Apply Font to All TextMeshPro"))
            ApplyFont();
    }
    #endregion

    #region Logic

    private void ApplyFont()
    {
        if (targetFont == null)
            return;

        ChangeInLoadedScenes();
        ChangeInPrefabs();

        AssetDatabase.SaveAssets();
        Debug.Log("Font replacement completed.");
    }

    private void ChangeInLoadedScenes()
    {
        var allTextComponents = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var text in allTextComponents)
        {
            if (text.font == targetFont)
                continue;

            Undo.RecordObject(text, "Change Font");
            text.font = targetFont;
            EditorUtility.SetDirty(text);
        }
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
            {
                if (text.font == targetFont)
                    continue;

                text.font = targetFont;
                EditorUtility.SetDirty(prefab);
            }
        }
    }
    #endregion
}