using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class EndScreen : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private TextMeshProUGUI timeResultText;
    [SerializeField] private MapState mapState;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private GlobalInventoryData globalInventory;
    [SerializeField] private GadgetsGlobalInventory gadgetsGlobalInventory;
    [SerializeField] private PauseMenuManager pauseMenuManager;

    [Header("Ending Cutscene")]
    [SerializeField] private VideoPlayer endingVideoPlayer;
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private UnityEngine.UI.Image skipProgressBar;
    [SerializeField] private float skipHoldDuration = 1.5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onCutsceneStarted;
    [SerializeField] private UnityEvent onWinScreenDisplayed;

    #endregion

    private bool isSequenceStarted;
    private Coroutine skipCoroutine;

    #region Unity Methods

    private void Start()
    {
        if (winScreenPanel)
            winScreenPanel.SetActive(false);

        if (cutscenePanel)
            cutscenePanel.SetActive(false);

        if (skipProgressBar)
            skipProgressBar.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (endingVideoPlayer)
        {
            endingVideoPlayer.prepareCompleted -= OnVideoPrepared;
            endingVideoPlayer.loopPointReached -= OnCutsceneFinished;
        }
    }

    #endregion

    #region Logic

    public void HandleBossDeath()
    {
        if (isSequenceStarted)
            return;

        isSequenceStarted = true;
        PlayEndingCutscene();
    }

    private void PlayEndingCutscene()
    {
        Time.timeScale = 0f;

        if (pauseMenuManager)
            pauseMenuManager.enabled = false;

        if (skipProgressBar)
        {
            skipProgressBar.fillAmount = 0f;
            skipProgressBar.gameObject.SetActive(true);
        }

        if (cutscenePanel)
            cutscenePanel.SetActive(true);

        if (gameplayUI)
            gameplayUI.SetActive(false);

        onCutsceneStarted?.Invoke();

        if (!endingVideoPlayer)
        {
            Debug.LogError("[EndScreen] EndingVideoPlayer is missing.");
            ShowWinScreen();
            return;
        }

        endingVideoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        Debug.Log("[EndScreen] Preparing video on unscaled time...");

        if (endingVideoPlayer.isPrepared)
            OnVideoPrepared(endingVideoPlayer);
        else
        {
            endingVideoPlayer.prepareCompleted += OnVideoPrepared;
            endingVideoPlayer.Prepare();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("[EndScreen] Video prepared, starting playback.");

        endingVideoPlayer.prepareCompleted -= OnVideoPrepared;
        endingVideoPlayer.loopPointReached += OnCutsceneFinished;
        endingVideoPlayer.Play();

        if (skipCoroutine != null)
            StopCoroutine(skipCoroutine);

        skipCoroutine = StartCoroutine(SkipLogicRoutine());
    }

    private IEnumerator SkipLogicRoutine()
    {
        float currentHoldTime = 0f;

        while (true)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                currentHoldTime += Time.unscaledDeltaTime;

                if (skipProgressBar)
                    skipProgressBar.fillAmount = currentHoldTime / skipHoldDuration;

                if (currentHoldTime >= skipHoldDuration)
                {
                    SkipCutscene();
                    yield break;
                }
            }
            else
            {
                currentHoldTime = 0f;

                if (skipProgressBar)
                    skipProgressBar.fillAmount = 0f;
            }

            yield return null;
        }
    }

    private void SkipCutscene()
    {
        if (endingVideoPlayer)
            endingVideoPlayer.Stop();

        Debug.Log("[EndScreen] Cutscene skipped by user.");
        ShowWinScreen();
    }

    private void OnCutsceneFinished(VideoPlayer vp) => ShowWinScreen();

    private void ShowWinScreen()
    {
        if (skipCoroutine != null)
            StopCoroutine(skipCoroutine);

        if (endingVideoPlayer)
            endingVideoPlayer.loopPointReached -= OnCutsceneFinished;

        if (cutscenePanel)
            cutscenePanel.SetActive(false);

        if (skipProgressBar)
            skipProgressBar.gameObject.SetActive(false);

        Time.timeScale = 0f;

        if (timeResultText != null)
        {
            if (GameTimerManager.Instance == null)
                timeResultText.text = "Time: --:--";
            else
                timeResultText.text = $"You managed to survive and bring The Core back to your lovely Mother. She was thrilled to finally have it in her grasp. All it took for her was to wait for:  <color=#875011>{GameTimerManager.Instance.GetFormattedTime()}</color>";
        }

        if (winScreenPanel)
            winScreenPanel.SetActive(true);

        onWinScreenDisplayed?.Invoke();
        ResetGameState();
    }

    private void ResetGameState()
    {
        if (mapState)
            mapState.Initialize();

        if (swarmState)
            swarmState.Initialize();

        if (globalInventory)
            globalInventory.Reset();

        if (gadgetsGlobalInventory)
            gadgetsGlobalInventory.Reset();
    }

    #endregion
}