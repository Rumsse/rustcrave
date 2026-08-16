using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public enum AAMode { Off, Low_FXAA, High_SMAA }
public enum VSyncMode { Off, On }

[Serializable]
public class SettingsData
{
    public float brightness = 0f;
    public AAMode antiAliasing = AAMode.High_SMAA;
    public int resolutionIndex = -1;
    public VSyncMode vsync = VSyncMode.On;
    public int fpsLimit = 60;
    public string inputOverrides = "";
}

[DefaultExecutionOrder(-100)]
public class SettingsManager : MonoBehaviour
{
    public static event Action<float> OnBrightnessChanged;
    public static SettingsManager Instance { get; private set; }

    public SettingsData CurrentSettings { get; private set; } = new SettingsData();

    [SerializeField] private InputActionAsset inputActions;
    public InputActionAsset InputActions => inputActions;

    private string SettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
        ApplyResolution();
        ApplyFramerateSettings();
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

    public void SetResolution(int index)
    {
        Resolution[] resolutions = Screen.resolutions;

        if (index < 0 || index >= resolutions.Length)
            return;

        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);

        CurrentSettings.resolutionIndex = index;
        SaveSettings();
    }

    public void SetVSync(VSyncMode mode)
    {
        CurrentSettings.vsync = mode;
        ApplyFramerateSettings();
        SaveSettings();
    }

    public void SetFPSLimit(int limit)
    {
        CurrentSettings.fpsLimit = limit;
        ApplyFramerateSettings();
        SaveSettings();
    }

    public void SaveInputOverrides()
    {
        if (!inputActions)
            return;

        CurrentSettings.inputOverrides = inputActions.SaveBindingOverridesAsJson();
        SaveSettings();
    }

    #endregion

    #region Logic

    private void ApplyResolution()
    {
        Resolution[] resolutions = Screen.resolutions;
        int resIndex = CurrentSettings.resolutionIndex;

        if (resIndex < 0 || resIndex >= resolutions.Length)
            return;

        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }

    private void ApplyAntiAliasingToCamera()
    {
        Camera mainCam = Camera.main;

        if (!mainCam)
            return;

        var cameraData = mainCam.GetComponent<UniversalAdditionalCameraData>();

        if (!cameraData)
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

    private void ApplyFramerateSettings()
    {
        QualitySettings.vSyncCount = CurrentSettings.vsync == VSyncMode.On ? 1 : 0;
        Application.targetFrameRate = CurrentSettings.fpsLimit;
    }

    private void ApplyInputOverrides()
    {
        if (!inputActions)
            return;

        if (string.IsNullOrEmpty(CurrentSettings.inputOverrides))
            return;

        inputActions.LoadBindingOverridesFromJson(CurrentSettings.inputOverrides);
    }

    #endregion

    #region Save & Load

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(CurrentSettings, true);
        File.WriteAllText(SettingsPath, json);
        Debug.Log("Settings saved globally.");
    }

    private void LoadSettings()
    {
        if (!File.Exists(SettingsPath))
        {
            Debug.LogWarning("No settings file found, using defaults.");
            CurrentSettings.resolutionIndex = Screen.resolutions.Length - 1;
            return;
        }

        string json = File.ReadAllText(SettingsPath);
        CurrentSettings = JsonUtility.FromJson<SettingsData>(json);

        if (CurrentSettings == null)
        {
            Debug.LogError("Failed to parse settings data.");
            CurrentSettings = new SettingsData();
        }

        if (CurrentSettings.resolutionIndex == -1 || CurrentSettings.resolutionIndex >= Screen.resolutions.Length)
            CurrentSettings.resolutionIndex = Screen.resolutions.Length - 1;

        ApplyInputOverrides();
    }

    #endregion
}