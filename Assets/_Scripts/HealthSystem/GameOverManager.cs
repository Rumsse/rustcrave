using UnityEngine;
using FMODUnity;

public class GameOverManager : MonoBehaviour
{
    #region Refs

    [SerializeField] private CanvasGroup backgroundCanvasGroup;
    [SerializeField] private CanvasGroup contentCanvasGroup;
    [SerializeField] private float backgroundFadeDuration = 1f;
    [SerializeField] private float contentFadeDuration = 2f;
    [SerializeField] private MapState mapState;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private PauseMenuManager pauseMenuManager;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private EventReference gameoverSound;

    private HealthManager playerHealth;

    #endregion

    #region Unity Methods

    private void Start()
    {
        if (backgroundCanvasGroup)
        {
            backgroundCanvasGroup.alpha = 0f;
            backgroundCanvasGroup.gameObject.SetActive(false);
        }

        if (contentCanvasGroup)
        {
            contentCanvasGroup.alpha = 0f;
            contentCanvasGroup.gameObject.SetActive(false);
            contentCanvasGroup.interactable = false;
            contentCanvasGroup.blocksRaycasts = false;
        }

        if (swarmState)
            swarmState.OnUnitRemoved += HandleUnitRemovedFromSwarm;

        InitializePlayerSubscription();
    }

    private void OnDestroy()
    {
        if (playerHealth)
            playerHealth.onDeath -= HandlePlayerDeath;

        if (swarmState)
            swarmState.OnUnitRemoved -= HandleUnitRemovedFromSwarm;
    }

    #endregion

    #region Core Logic

    private async void InitializePlayerSubscription()
    {
        GameObject player = null;

        while (!player)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
            await Awaitable.NextFrameAsync();
        }

        playerHealth = player.GetComponentInParent<HealthManager>();

        if (!playerHealth)
            return;

        playerHealth.onDeath += HandlePlayerDeath;
    }

    private void HandleUnitRemovedFromSwarm(SwarmUnitsData deadUnit)
    {
        if (deadUnit.unitType.isMainCharacter)
            HandlePlayerDeath();
    }

    private async void HandlePlayerDeath()
    {
        Time.timeScale = 0f;

        if (pauseMenuManager)
            pauseMenuManager.enabled = false;

        ResetGameState();

        if (!backgroundCanvasGroup || !contentCanvasGroup)
            return;

        backgroundCanvasGroup.gameObject.SetActive(true);
        contentCanvasGroup.gameObject.SetActive(true);

        AudioManager.PlayOneShot(gameoverSound);

        await FadeCanvasGroup(backgroundCanvasGroup, backgroundFadeDuration);

        await FadeCanvasGroup(contentCanvasGroup, contentFadeDuration);

        contentCanvasGroup.interactable = true;
        contentCanvasGroup.blocksRaycasts = true;
    }

    private async Awaitable FadeCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
            await Awaitable.NextFrameAsync();
        }

        canvasGroup.alpha = 1f;
    }

    private void ResetGameState()
    {
        if (mapState)
            mapState.Initialize();

        if (swarmState)
            swarmState.Initialize();
    }

    #endregion
}