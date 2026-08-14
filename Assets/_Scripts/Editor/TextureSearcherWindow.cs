using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TextureSearcherWindow : EditorWindow
{
    #region Fields

    private Texture2D searchTexture;
    private List<GameObject> foundObjects = new List<GameObject>();
    private Vector2 scrollPosition;

    #endregion

    #region Initialization

    [MenuItem("Tools/Texture Searcher")]
    public static void ShowWindow() => GetWindow<TextureSearcherWindow>("Texture Searcher");

    #endregion

    #region GUI

    private void OnGUI()
    {
        GUILayout.Label("Search Settings", EditorStyles.boldLabel);
        searchTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", searchTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Search in Active Scene"))
            PerformSearch();

        if (foundObjects.Count == 0)
            return;

        GUILayout.Space(10);
        GUILayout.Label("Found Objects:", EditorStyles.boldLabel);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var obj in foundObjects)
        {
            if (obj == null)
                continue;

            if (GUILayout.Button(obj.name))
                EditorGUIUtility.PingObject(obj);
        }

        GUILayout.EndScrollView();
    }

    #endregion

    #region Search Logic

    private void PerformSearch()
    {
        foundObjects.Clear();

        if (searchTexture == null)
        {
            Debug.LogWarning("Assign a texture before searching.");
            return;
        }

        var allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var go in allObjects)
        {
            if (HasTexture(go))
                foundObjects.Add(go);
        }

        Debug.Log($"Search complete. Found {foundObjects.Count} objects.");
    }

    private bool HasTexture(GameObject go)
    {
        if (CheckSpriteRenderer(go))
            return true;

        if (CheckImage(go))
            return true;

        if (CheckRawImage(go))
            return true;

        if (CheckParticleSystem(go))
            return true;

        if (CheckMeshRenderer(go))
            return true;

        return false;
    }

    #endregion

    #region Component Validators

    private bool CheckSpriteRenderer(GameObject go)
    {
        if (!go.TryGetComponent<SpriteRenderer>(out var sr))
            return false;

        if (sr.sprite == null)
            return false;

        return sr.sprite.texture == searchTexture;
    }

    private bool CheckImage(GameObject go)
    {
        if (!go.TryGetComponent<Image>(out var img))
            return false;

        if (img.sprite == null)
            return false;

        return img.sprite.texture == searchTexture;
    }

    private bool CheckRawImage(GameObject go)
    {
        if (!go.TryGetComponent<RawImage>(out var rawImg))
            return false;

        return rawImg.texture == searchTexture;
    }

    private bool CheckParticleSystem(GameObject go)
    {
        if (go.TryGetComponent<ParticleSystem>(out var ps))
        {
            var tsa = ps.textureSheetAnimation;
            if (tsa.enabled)
            {
                for (int i = 0; i < tsa.spriteCount; i++)
                {
                    var sprite = tsa.GetSprite(i);
                    if (sprite != null && sprite.texture == searchTexture)
                        return true;
                }
            }
        }

        if (!go.TryGetComponent<ParticleSystemRenderer>(out var psr))
            return false;

        var mat = psr.sharedMaterial;

        if (mat == null)
            return false;

        if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") == searchTexture)
            return true;

        if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") == searchTexture)
            return true;

        return false;
    }

    private bool CheckMeshRenderer(GameObject go)
    {
        if (!go.TryGetComponent<MeshRenderer>(out var mr))
            return false;

        foreach (var mat in mr.sharedMaterials)
        {
            if (mat == null)
                continue;

            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") == searchTexture)
                return true;

            if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") == searchTexture)
                return true;
        }

        return false;
    }

    #endregion
}