using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class BulkRenamerWindow : EditorWindow
{
    private string prefix = "";
    private string suffix = "";
    private string replaceFrom = "";
    private string replaceTo = "";
    private bool removeUnitySuffixes = false;

    [MenuItem("Tools/Bulk Renamer")]
    public static void ShowWindow() => GetWindow<BulkRenamerWindow>("Bulk Renamer");

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Select objects you want to rename in the Hierarchy or Project window first.", MessageType.Info);
        EditorGUILayout.Space();

        prefix = EditorGUILayout.TextField("Prefix", prefix);
        suffix = EditorGUILayout.TextField("Suffix", suffix);
        replaceFrom = EditorGUILayout.TextField("Replace", replaceFrom);
        replaceTo = EditorGUILayout.TextField("With", replaceTo);

        EditorGUILayout.Space();
        removeUnitySuffixes = EditorGUILayout.Toggle("Remove ' (1)' Suffixes", removeUnitySuffixes);
        EditorGUILayout.Space();

        if (GUILayout.Button("Rename Selected"))
            Rename();
    }

    private void Rename()
    {
        if (Selection.objects.Length == 0)
            return;

        string pattern = @" \([0-9]+\)$";

        foreach (Object obj in Selection.objects)
        {
            string newName = obj.name;
            string numericalSuffix = "";

            Match match = Regex.Match(newName, pattern);

            if (match.Success)
            {
                if (!removeUnitySuffixes)
                    numericalSuffix = match.Value;

                newName = newName.Substring(0, match.Index);
            }

            if (!string.IsNullOrEmpty(replaceFrom))
                newName = newName.Replace(replaceFrom, replaceTo);

            newName = prefix + newName + suffix + numericalSuffix;

            if (EditorUtility.IsPersistent(obj))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.RenameAsset(path, newName);
                continue;
            }

            Undo.RecordObject(obj, "Bulk Rename");
            obj.name = newName;
        }
    }
}