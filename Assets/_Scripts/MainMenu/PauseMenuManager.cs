using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    #region Events And Singleton

    public static PauseMenuManager Instance { get; private set; }
    public static event Action<bool> OnPauseStateChanged;

    #endregion

    #region Serialized Fields

    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private EventReference interactionSound;
    [SerializeField] private string sfxVcaPath = "vca:/SFX";

    #endregion

    #region Private Fields

    private const float MUTED_VOLUME = 0f;
    private VCA sfxVca;
    private float savedVcaVolume;

    #endregion

    #region Public Properties

    public bool IsPaused { get; private set; }

    #endregion

    #region Unity Methods

    private void Awake() => Instance = this;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        sfxVca = RuntimeManager.GetVCA(sfxVcaPath);
    }

    private void Update()
    {
        if (MainMenuManager.IsAnimating)
            return;

        if (Input.GetKeyDown(pauseKey))
            TogglePause();
    }

    #endregion

    #region Pause Logic

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        OnPauseStateChanged?.Invoke(true);

        sfxVca.getVolume(out savedVcaVolume);
        sfxVca.setVolume(MUTED_VOLUME);

        if (pausePanel == null)
            return;

        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialPaused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        OnPauseStateChanged?.Invoke(false);
        sfxVca.setVolume(savedVcaVolume);

        if (pausePanel == null)
            return;

        AudioManager.PlayOneShot(interactionSound);
        pausePanel.SetActive(false);
    }

    public async void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        sfxVca.setVolume(savedVcaVolume);
        AudioManager.PlayOneShot(interactionSound);

        if (SceneTransitionManager.Instance == null)
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        await SceneTransitionManager.Instance.FadeToScene(mainMenuSceneName);
    }

    #endregion
}