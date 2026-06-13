using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private Volume brightnessVolume;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Dropdown aaDropdown;

    #region Unity Lifecycle

    private void Start()
    {
        if (brightnessVolume == null || brightnessSlider == null || aaDropdown == null)
        {
            Debug.LogError("SettingsMenuController references are missing.");
            return;
        }

        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        aaDropdown.onValueChanged.AddListener(OnAAChanged);
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

        SettingsManager.OnBrightnessChanged -= UpdateVolume;
    }

    #endregion

    #region UI Callbacks

    private void OnBrightnessChanged(float value) => SettingsManager.Instance.SetBrightness(value);

    private void OnAAChanged(int index) => SettingsManager.Instance.SetAntiAliasing((AAMode)index);

    #endregion

    #region Logic

    private void UpdateVolume(float value) => brightnessVolume.weight = value;

    #endregion
}