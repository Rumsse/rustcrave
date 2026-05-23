using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum TutorialTaskType
{
    None,
    MoveUnits,
    GatherResources,
    KillEnemy,
    BreakWall,
    DetachSpider,
    EnterRestroom,
    InteractWithEvent,
    CraftRobot,
    CraftGadget,
    RestoreEnergy,
    EquipGadget,
    ScanPathAndGo,
    EndTutorial
}

[System.Serializable]
public class TutorialPage
{
    public string title;
    [TextArea(3, 10)]
    public string explanationText;
}

[System.Serializable]
public class TutorialStep
{
    public string stepName;
    public List<TutorialPage> pages;
    
    [Tooltip("Task that must be completed after reading this step")]
    public TutorialTaskType requiredTask;
    
    [Tooltip("Text shown on screen as a reminder while the task is active")]
    [TextArea] public string taskReminderText;
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public bool IsTutorialPaused => tutorialPanel != null && tutorialPanel.activeSelf;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text explanationText;
    [SerializeField] private TMP_Text pageNumberText;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button continueBtn;

    [Header("Task Reminder UI")]
    [SerializeField] private GameObject taskReminderPanel;
    [SerializeField] private TMP_Text taskReminderText;

    [Header("Tutorial Data")]
    [SerializeField] private List<TutorialStep> steps;

    [Header("Modifiers")]
    [SerializeField] private ActiveModifier globalActiveModifier;
    [SerializeField] private PathModifierData calmModifierData;

    [SerializeField] private string mainMenuSceneName = "Main Menu";
    /*[SerializeField]*/ private CameraZoom cameraZoom;

    private int currentStepIndex = 0;
    private int currentPageIndex = 0;
    

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        if (globalActiveModifier != null && calmModifierData != null)
            globalActiveModifier.Set(calmModifierData);

    }

    /*private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cameraZoom = FindAnyObjectByType<CameraZoom>();
    }*/

    private void Start()
    {
        cameraZoom = FindAnyObjectByType<CameraZoom>();

        if (previousBtn != null)
            previousBtn.onClick.AddListener(PrevPage);
        if (nextBtn != null)
            nextBtn.onClick.AddListener(NextPage);
        if (continueBtn != null)
            continueBtn.onClick.AddListener(OnContinueClick);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        if (taskReminderPanel != null)
            taskReminderPanel.SetActive(false);

        if (FindAnyObjectByType<TunnelProgressUI>() == null)
            ShowStep(0);
    }

    public void ShowStep(int index)
    {
        if (index >= steps.Count)
        {
            EndTutorial();
            return;
        }

        currentStepIndex = index;
        currentPageIndex = 0;
        
        Time.timeScale = 0f;
        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (steps == null || steps.Count == 0) return;

        TutorialStep currentStep = steps[currentStepIndex];
        if (currentStep.pages == null || currentStep.pages.Count == 0) return;

        TutorialPage currentPage = currentStep.pages[currentPageIndex];

        if (titleText != null) titleText.text = currentPage.title;
        if (explanationText != null) explanationText.text = currentPage.explanationText;
        if (pageNumberText != null) pageNumberText.text = $"{currentPageIndex + 1} / {currentStep.pages.Count}";

        if (previousBtn != null) previousBtn.gameObject.SetActive(currentPageIndex > 0);
        
        if (currentPageIndex < currentStep.pages.Count - 1)
        {
            if (nextBtn != null) nextBtn.gameObject.SetActive(true);
            if (continueBtn != null) continueBtn.gameObject.SetActive(false);
        }
        else
        {
            if (nextBtn != null) nextBtn.gameObject.SetActive(false);
            if (continueBtn != null) continueBtn.gameObject.SetActive(true);
        }
    }

    private void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateUI();
        }
    }

    private void NextPage()
    {
        if (currentPageIndex < steps[currentStepIndex].pages.Count - 1)
        {
            currentPageIndex++;
            UpdateUI();
        }
    }

    private void OnContinueClick()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
            
        Time.timeScale = 1f;

        TutorialStep currentStep = steps[currentStepIndex];

        if (cameraZoom != null && currentStep.requiredTask != TutorialTaskType.None)
            cameraZoom.IsPausedForTutorial = true;

        if (currentStep.requiredTask != TutorialTaskType.None && !string.IsNullOrEmpty(currentStep.taskReminderText))
        {
            if (taskReminderText != null)
                taskReminderText.text = currentStep.taskReminderText;
            if (taskReminderPanel != null)
                taskReminderPanel.SetActive(true);
        }

        if (TutorialTaskVerifier.Instance != null)
            TutorialTaskVerifier.Instance.StartTask(steps[currentStepIndex].requiredTask, CompleteCurrentTask);
        else
            CompleteCurrentTask();
    }

    public void CompleteCurrentTask()
    {
        if (taskReminderPanel != null)
            taskReminderPanel.SetActive(false);

        if (cameraZoom != null)
            cameraZoom.IsPausedForTutorial = false;
        
        ShowStep(currentStepIndex + 1);
    }

    private void EndTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
        
        if (cameraZoom != null)
            cameraZoom.IsPausedForTutorial = false;

        bool isFinalTutorialStep = false;
        if (steps != null && currentStepIndex >= 0 && currentStepIndex < steps.Count)
        {
            if (steps[currentStepIndex].requiredTask == TutorialTaskType.EndTutorial)
                isFinalTutorialStep = true;
        }

        if (isFinalTutorialStep)
        {
            currentStepIndex = 0;

            Debug.Log("Tutorial Finished! Loading Main Menu...");

            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }

}
