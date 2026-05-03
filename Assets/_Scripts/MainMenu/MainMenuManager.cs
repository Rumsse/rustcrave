using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName;
    [SerializeField] private string tutorialSceneName;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private MapState mapState;
    [SerializeField] private GlobalInventorySO globalInventory;
    [SerializeField] private GadgetsGlobalInventory gadgetsInventory;

    [Header("Sounds")]
    [SerializeField] private EventReference interactionButtonSound;

    private void Start()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void StartGame()
    {
        swarmState.Initialize();
        mapState.Initialize();
        globalInventory.Reset();
        gadgetsInventory.Reset();
        AudioManager.PlayOneShot(interactionButtonSound);
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartTutorial()
    {
        swarmState.Initialize();
        mapState.Initialize();
        globalInventory.Reset();
        gadgetsInventory.Reset();
        AudioManager.PlayOneShot(interactionButtonSound);
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void OpenCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void OpenGuide()
    {
        if (guidePanel != null)
            guidePanel.SetActive(true);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseGuide()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        AudioManager.PlayOneShot(interactionButtonSound);
        Application.Quit();
#endif
    }
}