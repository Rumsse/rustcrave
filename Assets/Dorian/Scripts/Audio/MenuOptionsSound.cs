using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class MenuOptionsSound : MonoBehaviour
{
    [SerializeField] private string musicVCAPath = "vca:/Amb";
    [SerializeField] private string sfxVCAPath = "vca:/SFX";

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private FMOD.Studio.VCA musicVCA;
    private FMOD.Studio.VCA sfxVCA;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const float DefaultVolume = 1f;

    private void Start()
    {
        musicVCA = RuntimeManager.GetVCA(musicVCAPath);
        sfxVCA = RuntimeManager.GetVCA(sfxVCAPath);

        InitializeSlider(musicSlider, MusicVolumeKey, musicVCA);
        InitializeSlider(sfxSlider, SfxVolumeKey, sfxVCA);
    }

    private void InitializeSlider(Slider slider, string prefsKey, FMOD.Studio.VCA vca)
    {
        if (slider == null)
            return;

        slider.value = PlayerPrefs.GetFloat(prefsKey, DefaultVolume);

        slider.onValueChanged.AddListener(value => SetVolume(vca, value, prefsKey));
    }

    private void SetVolume(FMOD.Studio.VCA vca, float volume, string prefsKey)
    {
        vca.setVolume(volume);
        PlayerPrefs.SetFloat(prefsKey, volume);
        PlayerPrefs.Save();
    }
}