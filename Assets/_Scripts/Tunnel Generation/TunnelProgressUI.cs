using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TunnelProgressUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Dependencies

    [SerializeField] MapState mapState;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] TunnelGenerator tunnelGenerator;
    [SerializeField] Transform cameraTargetTransform;
    [SerializeField] Transform startPos;

    #endregion

    #region State

    [SerializeField] Slider progressSlider;
    [SerializeField] TextMeshProUGUI stageInfoText;
    [SerializeField] GameObject tooltipPanel;
    [SerializeField] TextMeshProUGUI tooltipText;

    #endregion

    #region State

    float startPositionZ;
    float endPositionZ;
    bool isReady;

    #endregion

    #region Initialization

    void OnEnable()
    {
        if (tunnelGenerator != null)
            tunnelGenerator.OnNavMeshReady += SetupTracking;
    }

    void OnDisable()
    {
        if (tunnelGenerator != null)
            tunnelGenerator.OnNavMeshReady -= SetupTracking;
    }

    void SetupTracking()
    {
        var segments = tunnelGenerator.GetSpawnedSegments();

        if (segments.Count == 0)
            return;

        //startPositionZ = segments[0].transform.position.z;
        startPositionZ = startPos.transform.position.z;
        endPositionZ = segments[^1].transform.position.z - 10f;

        UpdateUIInfo();
        tooltipPanel.SetActive(false);
        isReady = true;
    }

    #endregion

    #region Core Logic

    void Update()
    {
        if (!isReady || cameraTargetTransform == null)
            return;

        float totalDistance = endPositionZ - startPositionZ;

        if (Mathf.Approximately(totalDistance, 0f))
            return;

        float currentDistance = cameraTargetTransform.position.z - startPositionZ;
        progressSlider.value = Mathf.Clamp01(currentDistance / totalDistance);
    }

    void UpdateUIInfo()
    {
        int currentStage = mapState.currentRow + 1;
        string modifierName = activeModifier.current != null ? activeModifier.current.name : "Standard";

        stageInfoText.text = $"CAVE {currentStage} - {modifierName}";
    }

    #endregion

    #region Tooltip Interfaces

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeModifier.current == null)
            return;

        tooltipText.text = $"Enemies: {activeModifier.EnemySpawnMultiplier}x\nResources: {activeModifier.ResourceSpawnMultiplier}x";
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) => tooltipPanel.SetActive(false);

    #endregion
}