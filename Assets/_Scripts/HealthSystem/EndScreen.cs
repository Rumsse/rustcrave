using UnityEngine;
using System.Collections;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private MapState mapState;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private GlobalInventorySO globalInventory;
    [SerializeField] private GadgetsGlobalInventory gadgetsGlobalInventory;
    [SerializeField] private PauseMenuManager pauseMenuManager;

    private void Start()
    {
        if (winScreenPanel)
            winScreenPanel.SetActive(false);
    }

    public void HandleBossDeath()
    {
        Debug.Log("Boss defeated! Displaying win screen...");

        Time.timeScale = 0f;

        if (pauseMenuManager)
            pauseMenuManager.enabled = false;

        if (winScreenPanel)
            winScreenPanel.SetActive(true);

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
}
