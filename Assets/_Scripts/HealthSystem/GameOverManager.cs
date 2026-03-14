using System.Collections;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private MapState mapState;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private PauseMenuManager pauseMenuManager;
    [SerializeField] private string playerTag = "Player";

    private HealthManager playerHealth;

    private void Start()
    {
        if (gameOverPanel)
            gameOverPanel.SetActive(false);

        StartCoroutine(WaitForPlayerAndSubscribe());
    }

    private IEnumerator WaitForPlayerAndSubscribe()
    {
        GameObject player = null;

        while (!player)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
            yield return null;
        }

        playerHealth = player.GetComponentInParent<HealthManager>();

        if (!playerHealth)
            yield break;

        playerHealth.onDeath += HandlePlayerDeath;
    }

    private void OnDestroy()
    {
        if (playerHealth)
            playerHealth.onDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        Time.timeScale = 0f;

        if (pauseMenuManager)
            pauseMenuManager.enabled = false;

        if (gameOverPanel)
            gameOverPanel.SetActive(true);

        ResetGameState();
    }

    private void ResetGameState()
    {
        if (mapState)
            mapState.Initialize();

        if (swarmState)
            swarmState.Initialize();
    }
}