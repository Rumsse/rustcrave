using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UIElements;

public class RestroomSceneController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private VideoPlayer introVideoPlayer;
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private UIDocument gameplayUI;
    [SerializeField] private float skipHoldDuration = 1.5f;
    [SerializeField] private UnityEngine.UI.Image skipProgressBar;

    #endregion

    private bool isCutscenePlaying;
    private float currentHoldTime;

    #region Unity Methods

    private void Start()
    {
        if (SceneLoadContext.IsNewGame)
            PlayIntroCutscene();
        else
            EnableGameplay();
    }

    private void Update()
    {
        if (!isCutscenePlaying)
            return;

        if (Input.GetKey(KeyCode.Space))
        {
            currentHoldTime += Time.deltaTime;

            if (skipProgressBar)
                skipProgressBar.fillAmount = currentHoldTime / skipHoldDuration;

            if (currentHoldTime >= skipHoldDuration)
                SkipCutscene();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            currentHoldTime = 0f;

            if (skipProgressBar)
                skipProgressBar.fillAmount = 0f;
        }
    }

    private void OnEnable() => PauseMenuManager.OnPauseStateChanged += HandlePauseState;

    private void OnDisable() => PauseMenuManager.OnPauseStateChanged -= HandlePauseState;

    private void OnDestroy()
    {
        if (introVideoPlayer)
            introVideoPlayer.loopPointReached -= OnCutsceneFinished;
    }

    #endregion

    #region Logic

    private void PlayIntroCutscene()
    {
        SceneLoadContext.IsNewGame = false;
        currentHoldTime = 0f;

        if (skipProgressBar)
        {
            skipProgressBar.fillAmount = 0f;
            skipProgressBar.gameObject.SetActive(true);
        }

        if (cutscenePanel)
            cutscenePanel.SetActive(true);

        StartCoroutine(HideUI());

        if (!introVideoPlayer)
        {
            Debug.LogError("[RestroomSceneController] IntroVideoPlayer is missing.");
            EnableGameplay();
            return;
        }

        if (SoundtrackPlayer.Instance)
            SoundtrackPlayer.Instance.SetAmbientVolume(0f);

        isCutscenePlaying = true;
        introVideoPlayer.loopPointReached += OnCutsceneFinished;
        introVideoPlayer.Play();
    }

    private IEnumerator HideUI()
    {
        yield return new WaitUntil(() => gameplayUI != null && gameplayUI.rootVisualElement != null);
        yield return new WaitForEndOfFrame();

        var root = gameplayUI.rootVisualElement;
        root.style.display = DisplayStyle.None;
        root.style.visibility = Visibility.Hidden;
        root.style.opacity = 0f;
    }

    private void SkipCutscene()
    {
        if (introVideoPlayer)
            introVideoPlayer.Stop();

        Debug.Log("[RestroomSceneController] Cutscene skipped by user.");
        EnableGameplay();
    }

    private void HandlePauseState(bool isPaused)
    {
        if (!isCutscenePlaying || !introVideoPlayer)
            return;

        if (isPaused)
            introVideoPlayer.Pause();
        else
            introVideoPlayer.Play();
    }

    private void OnCutsceneFinished(VideoPlayer vp) => EnableGameplay();

    private void EnableGameplay()
    {
        isCutscenePlaying = false;

        if (skipProgressBar)
            skipProgressBar.gameObject.SetActive(false);

        if (cutscenePanel)
            cutscenePanel.SetActive(false);

        if (gameplayUI && gameplayUI.rootVisualElement != null)
        {
            var root = gameplayUI.rootVisualElement;
            root.style.display = DisplayStyle.Flex;
            root.style.visibility = Visibility.Visible;
            root.style.opacity = 1f;
        }

        if (SoundtrackPlayer.Instance)
            SoundtrackPlayer.Instance.SetAmbientVolume(1f);

        Debug.Log("[RestroomSceneController] Gameplay initialized.");
    }

    #endregion
}