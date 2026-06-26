using UnityEngine;
using FMODUnity;

public class AudioSettingsInitializer : MonoBehaviour
{
    [SerializeField] private string musicVCAPath = "vca:/Amb";
    [SerializeField] private string sfxVCAPath = "vca:/SFX";

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const float DefaultVolume = 1f;

    private void Start()
    {
        LoadVolume(musicVCAPath, MusicVolumeKey);
        LoadVolume(sfxVCAPath, SfxVolumeKey);
    }

    private void LoadVolume(string vcaPath, string prefsKey)
    {
        FMOD.Studio.VCA vca = RuntimeManager.GetVCA(vcaPath);
        float savedVolume = PlayerPrefs.GetFloat(prefsKey, DefaultVolume);
        vca.setVolume(savedVolume);
    }
}