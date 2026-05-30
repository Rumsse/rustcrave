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

    #region Unity Methods

    private void Start()
    {
        if (optionsPanelMainMenuOnly)
            optionsPanelMainMenuOnly.SetActive(false);

        if (creditsPanel)
            creditsPanel.SetActive(false);
    }

    #endregion

    #region Menu Actions

    public void StartGame() => StartPlaySequence(gameSceneName);

    public void StartTutorial() => StartPlaySequence(tutorialSceneName);

    public void QuitGame()
    {
        if (quittingPanel)
            quittingPanel.SetActive(false);

        StartCoroutine(MoveCharacterAndExecute(quitGameTarget, QuitApplication));
    }

    public void OpenOptions() => StartCoroutine(PlayAnimationAndExecute("CAPOFF 0", () => ActivatePanel(optionsPanelMainMenuOnly)));

    public void CloseOptions() => SetPanelState(optionsPanelMainMenuOnly, false);

    public void OpenCredits() => StartCoroutine(PlayAnimationAndExecute("CAPOFF 0", StartCreditsSequence));

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
        AudioManager.PlayOneShot(interactionButtonSound);

        if (!characterAnimator)
        {
            Debug.LogError("Missing Animator reference.");
            onComplete?.Invoke();
            yield break;
        }

        characterAnimator.Play(animationStateName);

        yield return new WaitForSeconds(panelAnimationDuration);

        onComplete?.Invoke();
    }

    private IEnumerator MoveCharacterAndExecute(Transform target, Action onComplete)
    {
        AudioManager.PlayOneShot(interactionButtonSound);

        if (!characterAnimator || !characterTransform || !target)
        {
            Debug.LogError("Missing references for character movement.");
            onComplete?.Invoke();
            yield break;
        }

        characterTransform.rotation = Quaternion.LookRotation(target.position - characterTransform.position);
        characterAnimator.SetTrigger("PrepareToWalk");

        yield return new WaitForSeconds(prepareAnimationDuration);

        characterAnimator.SetBool("IsWalking", true);

        while (Vector3.Distance(characterTransform.position, target.position) > 0.1f)
        {
            characterTransform.position = Vector3.MoveTowards(characterTransform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        onComplete?.Invoke();
    }

    private void InitializeGameStates()
    {
        if (GameTimerManager.Instance != null)
        {
            GameTimerManager.Instance.ResetTimer();
            GameTimerManager.Instance.StartTimer();
        }

        swarmState.Initialize();
        mapState.Initialize();
        globalInventory.Reset();
        gadgetsInventory.Reset();
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void StopTimer()
    {
        if (GameTimerManager.Instance != null)
            GameTimerManager.Instance.StopTimer();
    }

    #endregion
}