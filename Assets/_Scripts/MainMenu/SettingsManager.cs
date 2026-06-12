using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class SettingsManager : MonoBehaviour
{
    public static event Action<float> OnBrightnessChanged;
    public static SettingsManager Instance { get; private set; }

    string SettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");

    public SettingsData CurrentSettings { get; private set; } = new SettingsData();

    #region Unity Lifecycle

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyAntiAliasingToCamera();

    #endregion

    #region API

    public void SetBrightness(float value)
    {
        CurrentSettings.brightness = value;
        OnBrightnessChanged?.Invoke(value);
        SaveSettings();
    }

    public void SetAntiAliasing(AAMode mode)
    {
        CurrentSettings.antiAliasing = mode;
        ApplyAntiAliasingToCamera();
        SaveSettings();
    }

    #endregion

    #region Logic

    private void ApplyAntiAliasingToCamera()
    {
        Camera mainCam = Camera.main;

        if (mainCam == null)
            return;

        var cameraData = mainCam.GetComponent<UniversalAdditionalCameraData>();

        if (cameraData == null)
        {
            Debug.LogWarning("Main Camera is missing UniversalAdditionalCameraData.");
            return;
        }

        switch (CurrentSettings.antiAliasing)
        {
            case AAMode.Off:
                cameraData.antialiasing = AntialiasingMode.None;
                break;
            case AAMode.Low_FXAA:
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                break;
            case AAMode.High_SMAA:
                cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                break;
        }
    }

    #endregion

    #region Save & Load

    void SaveSettings()
    {
        string json = JsonUtility.ToJson(CurrentSettings, true);
        File.WriteAllText(SettingsPath, json);
        Debug.Log("[SettingsManager] Settings saved globally.");
    }

    void LoadSettings()
    {
        if (!File.Exists(SettingsPath))
        {
            Debug.LogWarning("[SettingsManager] No settings file found, using defaults.");
            return;
        }

        string json = File.ReadAllText(SettingsPath);
        CurrentSettings = JsonUtility.FromJson<SettingsData>(json);

        if (CurrentSettings == null)
        {
            Debug.LogError("[SettingsManager] Failed to parse settings data!");
            CurrentSettings = new SettingsData();
        }
    }

    #endregion
}

public enum AAMode { Off, Low_FXAA, High_SMAA }

[Serializable]
public class SettingsData
{
    public float brightness = 0f;
    public AAMode antiAliasing = AAMode.High_SMAA; 
}