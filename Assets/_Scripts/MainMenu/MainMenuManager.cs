using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class MainMenuManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private string gameSceneName;
    [SerializeField] private string tutorialSceneName;
    [SerializeField] private GameObject optionsPanelMainMenuOnly;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsSoundsPanel;
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private GameObject quittingPanel;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private MapState mapState;
    [SerializeField] private GlobalInventorySO globalInventory;
    [SerializeField] private GadgetsGlobalInventory gadgetsInventory;

    [Header("Character Movement")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private Transform characterTransform;
    [SerializeField] private Transform startGameTarget;
    [SerializeField] private Transform quitGameTarget;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float prepareAnimationDuration = 1.5f;

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

    public void StartGame()
    {
        InitializeGameStates();
        StartCoroutine(MoveCharacterAndExecute(startGameTarget, () => SceneManager.LoadScene(gameSceneName)));
    }

    public void StartTutorial()
    {
        InitializeGameStates();
        StartCoroutine(MoveCharacterAndExecute(startGameTarget, () => SceneManager.LoadScene(tutorialSceneName)));
    }

    public void QuitGame()
    {
        if (quittingPanel)
            quittingPanel.SetActive(false);

        StartCoroutine(MoveCharacterAndExecute(quitGameTarget, QuitApplication));
    }

    public void OpenOptions()
    {
        if (optionsPanelMainMenuOnly)
            optionsPanelMainMenuOnly.SetActive(true);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseOptions()
    {
        if (optionsPanelMainMenuOnly)
            optionsPanelMainMenuOnly.SetActive(false);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void OpenCredits()
    {
        if (creditsPanel)
            creditsPanel.SetActive(true);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseCredits()
    {
        if (creditsPanel)
            creditsPanel.SetActive(false);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void OpenSettings()
    {
        if (settingsSoundsPanel)
            settingsSoundsPanel.SetActive(true);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseSettings()
    {
        if (settingsSoundsPanel)
            settingsSoundsPanel.SetActive(false);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void OpenGuide()
    {
        if (guidePanel)
            guidePanel.SetActive(true);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseGuide()
    {
        if (guidePanel)
            guidePanel.SetActive(false);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void OpenQuittingPanel()
    {
        if (quittingPanel)
            quittingPanel.SetActive(true);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    public void CloseQuittingPanel()
    {
        if (quittingPanel)
            quittingPanel.SetActive(false);

        AudioManager.PlayOneShot(interactionButtonSound);
    }

    #endregion

    #region Coroutines And Logic

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

    #endregion
}