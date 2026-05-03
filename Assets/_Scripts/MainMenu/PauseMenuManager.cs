using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class PauseMenuManager : MonoBehaviour
{
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

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            AudioManager.PlayOneShot(interactionSound);
            pausePanel.SetActive(false);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.PlayOneShot(interactionSound);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}