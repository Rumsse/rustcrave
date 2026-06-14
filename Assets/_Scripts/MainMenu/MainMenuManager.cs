using System;
using System.Collections;
using UnityEngine;
using FMODUnity;

public class MainMenuManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private string gameSceneName;
    [SerializeField] private string tutorialSceneName;
    [SerializeField] private GameObject optionsPanelMainMenuOnly;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private CreditsController creditsController;
    [SerializeField] private GameObject settingsSoundsPanel;
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private GameObject quittingPanel;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private MapState mapState;
    [SerializeField] private GlobalInventorySO globalInventory;
    [SerializeField] private GadgetsGlobalInventory gadgetsInventory;
    [SerializeField] private EventState eventState;
    [SerializeField] private AnimatedTextButton continueButton;

    [Header("Character Movement & Animations")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private Transform characterTransform;
    [SerializeField] private Transform startGameTarget;
    [SerializeField] private Transform quitGameTarget;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float prepareAnimationDuration = 1.5f;
    [SerializeField] private float panelAnimationDuration = 1f;

    [Header("Sounds")]
    [SerializeField] private EventReference interactionButtonSound;

    #endregion

    #region Public Properties

    public static bool IsAnimating { get; private set; }

    #endregion

    #region Unity Methods

    private void Start()
    {
        if (optionsPanelMainMenuOnly)
            optionsPanelMainMenuOnly.SetActive(false);

        if (creditsPanel)
            creditsPanel.SetActive(false);

        if (continueButton != null)
            continueButton.SetInteractable(SaveManager.Instance.HasAnySave());
    }

    private void OnDisable() => IsAnimating = false;

    #endregion

    #region Menu Actions

    public void StartNewMission()
    {
        if (IsAnimating)
            return;

        SceneLoadContext.IsNewGame = true;
        InitializeGameStates();
        StartCoroutine(MoveCharacterAndExecute(startGameTarget, () => _ = SceneTransitionManager.Instance.WipeToScene(gameSceneName)));
    }

    public void ContinueMission()
    {
        if (IsAnimating)
            return;

        if (!SaveManager.Instance.HasAnySave())
        {
            Debug.Log("No saves found to continue.");
            return;
        }

        SaveManager.Instance.ContinueGame();
        StartCoroutine(MoveCharacterAndExecute(startGameTarget, () => _ = SceneTransitionManager.Instance.WipeToScene(gameSceneName)));
    }

    public void LoadMissionFromSlot(int slotIndex)
    {
        if (IsAnimating)
            return;

        SaveManager.Instance.LoadGame(slotIndex);
        StartCoroutine(MoveCharacterAndExecute(startGameTarget, () => _ = SceneTransitionManager.Instance.WipeToScene(gameSceneName)));
    }

    public void StartGame() => StartPlaySequence(gameSceneName);

    public void StartTutorial() => StartPlaySequence(tutorialSceneName);

    public void QuitGame()
    {
        if (IsAnimating)
            return;

        if (quittingPanel)
            quittingPanel.SetActive(false);

        StartCoroutine(MoveCharacterAndExecute(quitGameTarget, QuitApplication));
    }

    public void OpenOptions()
    {
        if (IsAnimating)
            return;

        StartCoroutine(PlayAnimationAndExecute("CAPOFF 0", () => ActivatePanel(optionsPanelMainMenuOnly)));
    }

    public void CloseOptions()
    {
        if (PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused)
            PauseMenuManager.Instance.Resume();
        else
            SetPanelState(optionsPanelMainMenuOnly, false);
    }

    public void OpenCredits()
    {
        if (IsAnimating)
            return;

        StartCoroutine(PlayAnimationAndExecute("CAPOFF 0", StartCreditsSequence));
    }

    public void CloseCredits()
    {
        SetPanelState(creditsPanel, false);

        if (creditsController)
            creditsController.Stop();
    }

    public void OpenSettings() => SetPanelState(settingsSoundsPanel, true);

    public void CloseSettings() => SetPanelState(settingsSoundsPanel, false);

    public void OpenGuide() => SetPanelState(guidePanel, true);

    public void CloseGuide() => SetPanelState(guidePanel, false);

    public void OpenQuittingPanel() => SetPanelState(quittingPanel, true);

    public void CloseQuittingPanel() => SetPanelState(quittingPanel, false);

    #endregion

    #region Logic And Coroutines

    private void StartPlaySequence(string sceneName)
    {
        if (IsAnimating)
            return;

        InitializeGameStates();
        StartCoroutine(MoveCharacterAndExecute(startGameTarget, () => _ = SceneTransitionManager.Instance.WipeToScene(sceneName)));
    }

    private void StartCreditsSequence()
    {
        ActivatePanel(creditsPanel);

        if (creditsController)
            creditsController.Play();
    }

    private void ActivatePanel(GameObject panel)
    {
        if (panel)
            panel.SetActive(true);
    }

    private void SetPanelState(GameObject panel, bool state)
    {
        if (panel)
            panel.SetActive(state);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    private IEnumerator PlayAnimationAndExecute(string animationStateName, Action onComplete)
    {
        IsAnimating = true;
        AudioManager.PlayOneShot(interactionButtonSound);

        if (!characterAnimator)
        {
            Debug.LogError("Missing Animator reference.");
            IsAnimating = false;
            onComplete?.Invoke();
            yield break;
        }

        characterAnimator.Play(animationStateName);
        yield return new WaitForSecondsRealtime(panelAnimationDuration);

        IsAnimating = false;
        onComplete?.Invoke();
    }

    private IEnumerator MoveCharacterAndExecute(Transform target, Action onComplete)
    {
        IsAnimating = true;
        AudioManager.PlayOneShot(interactionButtonSound);

        if (!characterAnimator || !characterTransform || !target)
        {
            Debug.LogError("Missing references for character movement.");
            IsAnimating = false;
            onComplete?.Invoke();
            yield break;
        }

        characterTransform.rotation = Quaternion.LookRotation(target.position - characterTransform.position);
        characterAnimator.SetTrigger("PrepareToWalk");

        yield return new WaitForSecondsRealtime(prepareAnimationDuration);

        characterAnimator.SetBool("IsWalking", true);

        while (Vector3.Distance(characterTransform.position, target.position) > 0.1f)
        {
            characterTransform.position = Vector3.MoveTowards(characterTransform.position, target.position, moveSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        IsAnimating = false;
        onComplete?.Invoke();
    }

    public void InitializeGameStates()
    {
        if (GameTimerManager.Instance != null)
        {
            GameTimerManager.Instance.ResetTimer();
            GameTimerManager.Instance.StartTimer();
        }

        Time.timeScale = 1f;

        swarmState.Initialize();
        mapState.Initialize();
        globalInventory.Reset();
        gadgetsInventory.Reset();
        eventState.Reset();
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void StopTimer()
    {
        if (GameTimerManager.Instance != null)
            GameTimerManager.Instance.StopTimer();
    }

    #endregion

    #region Save System UI

    public void OnContinueClicked()
    {
        if (!SaveManager.Instance.HasAnySave() || IsAnimating)
            return;

        SaveManager.Instance.ContinueGame();
        StartGame();
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (IsAnimating)
            return;

        if (SaveManager.Instance.HasSaveFile(slotIndex))
        {
            SaveManager.Instance.SetCurrentSlot(slotIndex);
            SaveManager.Instance.SaveGame();
        }
        else
        {
            SaveManager.Instance.SetCurrentSlot(slotIndex);
            SceneLoadContext.IsNewGame = true;
            InitializeGameStates();
            StartGame();
        }
    }

    public void UpdateSlotUI(TMPro.TextMeshProUGUI textSlot1, TMPro.TextMeshProUGUI textSlot2, TMPro.TextMeshProUGUI textSlot3)
    {
        textSlot1.text = $"Save 1\n{SaveManager.Instance.GetSlotDate(1)}";
        textSlot2.text = $"Save 2\n{SaveManager.Instance.GetSlotDate(2)}";
        textSlot3.text = $"Save 3\n{SaveManager.Instance.GetSlotDate(3)}";
    }

    #endregion
}