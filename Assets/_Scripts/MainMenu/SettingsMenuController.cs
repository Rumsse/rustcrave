using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    #region Configuration

    [SerializeField] private Volume brightnessVolume;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Dropdown aaDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown fpsLimitDropdown;
    [SerializeField] private GameObject bindingsPanel;

    #endregion

    #region State

    private Resolution[] resolutions;
    private readonly int[] fpsLimits = { 30, 60, 120, 144, 240, -1 };

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (!brightnessVolume || !brightnessSlider || !aaDropdown || !resolutionDropdown || !vsyncToggle || !fpsLimitDropdown)
        {
            Debug.LogError("SettingsMenuController references are missing.");
            return;
        }

        resolutions = Screen.resolutions;
        InitializeResolutionDropdown();
        InitializeFPSDropdown();

        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        aaDropdown.onValueChanged.AddListener(OnAAChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        fpsLimitDropdown.onValueChanged.AddListener(OnFPSLimitChanged);

        SettingsManager.OnBrightnessChanged += UpdateVolume;

        float savedBrightness = SettingsManager.Instance.CurrentSettings.brightness;
        brightnessSlider.value = savedBrightness;
        UpdateVolume(savedBrightness);

        aaDropdown.value = (int)SettingsManager.Instance.CurrentSettings.antiAliasing;
        vsyncToggle.isOn = SettingsManager.Instance.CurrentSettings.vsync == VSyncMode.On;
        SetFPSDropdownValue(SettingsManager.Instance.CurrentSettings.fpsLimit);

        if (bindingsPanel)
            bindingsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (brightnessSlider)
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);

        if (aaDropdown)
            aaDropdown.onValueChanged.RemoveListener(OnAAChanged);

        if (resolutionDropdown)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        if (vsyncToggle)
            vsyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);

        if (fpsLimitDropdown)
            fpsLimitDropdown.onValueChanged.RemoveListener(OnFPSLimitChanged);

        SettingsManager.OnBrightnessChanged -= UpdateVolume;
    }

    #endregion

    #region UI Callbacks

    public void OpenBindingsPanel()
    {
        if (!bindingsPanel)
        {
            Debug.LogWarning("Bindings Panel reference is missing in SettingsMenuController!");
            return;
        }

        bindingsPanel.SetActive(true);
    }

    public void CloseBindingsPanel()
    {
        if (!bindingsPanel)
            return;

        bindingsPanel.SetActive(false);
    }

    private void OnBrightnessChanged(float value) => SettingsManager.Instance.SetBrightness(value);

    private void OnAAChanged(int index) => SettingsManager.Instance.SetAntiAliasing((AAMode)index);

    private void OnResolutionChanged(int index) => SettingsManager.Instance.SetResolution(index);

    private void OnVSyncChanged(bool isOn) => SettingsManager.Instance.SetVSync(isOn ? VSyncMode.On : VSyncMode.Off);

    private void OnFPSLimitChanged(int index) => SettingsManager.Instance.SetFPSLimit(fpsLimits[index]);

    #endregion

    #region Logic

    private void UpdateVolume(float value) => brightnessVolume.weight = value;

    private void InitializeResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int savedIndex = SettingsManager.Instance.CurrentSettings.resolutionIndex;
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");

            if (savedIndex == i)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void InitializeFPSDropdown()
    {
        fpsLimitDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < fpsLimits.Length; i++)
        {
            if (fpsLimits[i] == -1)
            {
                options.Add("Unlimited");
                continue;
            }

            options.Add(fpsLimits[i].ToString());
        }

        fpsLimitDropdown.AddOptions(options);
    }

    private void SetFPSDropdownValue(int currentLimit)
    {
        for (int i = 0; i < fpsLimits.Length; i++)
        {
            if (fpsLimits[i] != currentLimit)
                continue;

            fpsLimitDropdown.value = i;
            fpsLimitDropdown.RefreshShownValue();
            return;
        }
    }

    #endregion
}