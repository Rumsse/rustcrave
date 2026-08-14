using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private Volume brightnessVolume;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Dropdown aaDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown fpsLimitDropdown;

    [Header("Bindings Panel")]
    [SerializeField] private GameObject bindingsPanel;
    [SerializeField] private Button openBindingsButton;
    [SerializeField] private Button closeBindingsButton;

    private Resolution[] resolutions;
    private readonly int[] fpsLimits = { 30, 60, 120, 144, 240, -1 };

    private void Start()
    {
        if (brightnessVolume == null || brightnessSlider == null || aaDropdown == null || resolutionDropdown == null || vsyncToggle == null || fpsLimitDropdown == null)
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

        if (openBindingsButton)
            openBindingsButton.onClick.AddListener(OpenBindingsPanel);

        if (closeBindingsButton)
            closeBindingsButton.onClick.AddListener(CloseBindingsPanel);

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

        if (openBindingsButton)
            openBindingsButton.onClick.RemoveListener(OpenBindingsPanel);

        if (closeBindingsButton)
            closeBindingsButton.onClick.RemoveListener(CloseBindingsPanel);

        SettingsManager.OnBrightnessChanged -= UpdateVolume;
    }

    public void OpenBindingsPanel()
    {
        if (bindingsPanel)
            bindingsPanel.SetActive(true);
    }

    public void CloseBindingsPanel()
    {
        if (bindingsPanel)
            bindingsPanel.SetActive(false);
    }

    private void OnBrightnessChanged(float value) => SettingsManager.Instance.SetBrightness(value);

    private void OnAAChanged(int index) => SettingsManager.Instance.SetAntiAliasing((AAMode)index);

    private void OnResolutionChanged(int index) => SettingsManager.Instance.SetResolution(index);

    private void OnVSyncChanged(bool isOn) => SettingsManager.Instance.SetVSync(isOn ? VSyncMode.On : VSyncMode.Off);

    private void OnFPSLimitChanged(int index) => SettingsManager.Instance.SetFPSLimit(fpsLimits[index]);

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
}