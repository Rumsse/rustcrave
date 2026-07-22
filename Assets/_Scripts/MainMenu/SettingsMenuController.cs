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

    private Resolution[] resolutions;

    #region Unity Lifecycle

    private void Start()
    {
        if (brightnessVolume == null || brightnessSlider == null || aaDropdown == null || resolutionDropdown == null)
        {
            Debug.LogError("SettingsMenuController references are missing.");
            return;
        }

        resolutions = Screen.resolutions;
        InitializeResolutionDropdown();

        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        aaDropdown.onValueChanged.AddListener(OnAAChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        SettingsManager.OnBrightnessChanged += UpdateVolume;

        float savedBrightness = SettingsManager.Instance.CurrentSettings.brightness;
        brightnessSlider.value = savedBrightness;
        UpdateVolume(savedBrightness);

        aaDropdown.value = (int)SettingsManager.Instance.CurrentSettings.antiAliasing;
    }

    private void OnDestroy()
    {
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);

        if (aaDropdown != null)
            aaDropdown.onValueChanged.RemoveListener(OnAAChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        SettingsManager.OnBrightnessChanged -= UpdateVolume;
    }

    #endregion

    #region UI Callbacks

    private void OnBrightnessChanged(float value) => SettingsManager.Instance.SetBrightness(value);

    private void OnAAChanged(int index) => SettingsManager.Instance.SetAntiAliasing((AAMode)index);

    private void OnResolutionChanged(int index) => SettingsManager.Instance.SetResolution(index);

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
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (savedIndex == i)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    #endregion
}