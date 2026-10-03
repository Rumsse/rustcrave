using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Scene Selector", true)]
public class SceneSelectorOverlay : Overlay
{
    private DropdownField _sceneDropdown;

    public override VisualElement CreatePanelContent()
    {
        _sceneDropdown = new DropdownField { tooltip = "Quick Scene Selector", style = { width = 160 } };
        UpdateScenes();

        _sceneDropdown.RegisterValueChangedCallback(OnSceneSelected);
        _sceneDropdown.RegisterCallback<AttachToPanelEvent>(OnAttach);
        _sceneDropdown.RegisterCallback<DetachFromPanelEvent>(OnDetach);

        return _sceneDropdown;
    }

    private void OnAttach(AttachToPanelEvent evt)
    {
        EditorBuildSettings.sceneListChanged += UpdateScenes;
        EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
    }

    private void OnDetach(DetachFromPanelEvent evt)
    {
        EditorBuildSettings.sceneListChanged -= UpdateScenes;
        EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
    }

    private void UpdateScenes()
    {
        if (_sceneDropdown == null)
            return;

        var scenes = GetBuildScenes();
        _sceneDropdown.choices = scenes;
        _sceneDropdown.value = scenes.Count > 0 ? GetCurrentSceneName() : "No Build Scenes";
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (_sceneDropdown != null)
            _sceneDropdown.SetValueWithoutNotify(next.name);
    }

    private List<string> GetBuildScenes() => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => Path.GetFileNameWithoutExtension(s.path)).ToList();

    private string GetCurrentSceneName() => SceneManager.GetActiveScene().name;

    private void OnSceneSelected(ChangeEvent<string> evt)
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Cannot switch scenes while in Play Mode.");
            _sceneDropdown.SetValueWithoutNotify(GetCurrentSceneName());
            return;
        }

        var selectedScene = EditorBuildSettings.scenes.FirstOrDefault(s => Path.GetFileNameWithoutExtension(s.path) == evt.newValue);
        if (selectedScene == null)
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            _sceneDropdown.SetValueWithoutNotify(GetCurrentSceneName());
            return;
        }

        EditorSceneManager.OpenScene(selectedScene.path);
    }
}