using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartTutorial()
    {
        swarmState.Initialize();
        mapState.Initialize();
        globalInventory.Reset();
        gadgetsInventory.Reset();
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }   

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenGuide()
    {
        if (guidePanel != null)
            guidePanel.SetActive(true);
    }

    public void CloseGuide()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}