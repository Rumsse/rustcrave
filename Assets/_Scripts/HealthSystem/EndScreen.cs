using System.Collections;
using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private TextMeshProUGUI timeResultText;
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

        if (timeResultText == null)
            return;

        if (GameTimerManager.Instance == null)
        {
            timeResultText.text = "Time: --:--";
            return;
        }

        timeResultText.text = $"You managed to survive and bring The Core back to your lovely Mother. She was thrilled to finally have it in her grasp. All it took for her was to wait for:  <color=#FFD700>{GameTimerManager.Instance.GetFormattedTime()}</color>";

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
