using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class GlobalPanelSettingsAssigner : EditorWindow
{
    #region Setup

    private PanelSettings targetSettings;

    [MenuItem("Tools/Assign Global Panel Settings")]
    public static void ShowWindow() => GetWindow<GlobalPanelSettingsAssigner>("Panel Assigner");

    private void OnGUI()
    {
        targetSettings = (PanelSettings)EditorGUILayout.ObjectField("Panel Settings", targetSettings, typeof(PanelSettings), false);

        if (GUILayout.Button("Assign to All UIDocuments"))
            AssignSettings();
    }
    #endregion

    #region Logic

    private void AssignSettings()
    {
        if (targetSettings == null)
            return;

        ChangeInLoadedScenes();
        ChangeInPrefabs();

        AssetDatabase.SaveAssets();
        Debug.Log("Panel Settings assignment completed.");
    }

    private void ChangeInLoadedScenes()
    {
        var allDocuments = FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var doc in allDocuments)
        {
            if (doc.panelSettings == targetSettings)
                continue;

            Undo.RecordObject(doc, "Change Panel Settings");
            doc.panelSettings = targetSettings;
            EditorUtility.SetDirty(doc);
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

            var documents = prefab.GetComponentsInChildren<UIDocument>(true);

            if (documents.Length == 0)
                continue;

            foreach (var doc in documents)
            {
                if (doc.panelSettings == targetSettings)
                    continue;

                doc.panelSettings = targetSettings;
                EditorUtility.SetDirty(prefab);
            }
        }
    }
    #endregion
}