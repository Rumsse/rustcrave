using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static event Action<bool> OnPauseStateChanged;

    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private EventReference interactionSound;

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (MainMenuManager.IsAnimating)
            return;

        if (Input.GetKeyDown(pauseKey))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        OnPauseStateChanged?.Invoke(true);

        if (pausePanel == null)
            return;

        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialPaused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        OnPauseStateChanged?.Invoke(false);

        if (pausePanel == null)
            return;

        AudioManager.PlayOneShot(interactionSound);
        pausePanel.SetActive(false);
    }

    public async void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.PlayOneShot(interactionSound);

        if (SceneTransitionManager.Instance == null)
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        await SceneTransitionManager.Instance.FadeToScene(mainMenuSceneName);
    }
}