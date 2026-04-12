using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PrimeTween;

public class TunnelProgressUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Dependencies

    [SerializeField] MapState mapState;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] TunnelGenerator tunnelGenerator;
    [SerializeField] Transform cameraTargetTransform;
    [SerializeField] Transform startPos;

    #endregion

    #region UI Elements

    [SerializeField] Slider progressSlider;
    [SerializeField] TextMeshProUGUI stageInfoText;
    [SerializeField] GameObject tooltipPanel;
    [SerializeField] TextMeshProUGUI tooltipText;
    [SerializeField] Image darkScreen;

    #endregion

    #region State

    float startPositionZ;
    float inverseTotalDistance;
    float lastSliderValue = -1f;
    bool isReady;
    Vector2 targetTextPosition;

    #endregion

    #region Initialization

    private void Awake() => Time.timeScale = 0f;

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

    void Start() => StartIntroSequence();

    void SetupTracking()
    {
        var segments = tunnelGenerator.GetSpawnedSegments();

        if (segments.Count == 0)
            return;

        startPositionZ = startPos.transform.position.z;
        float endPositionZ = segments[^1].transform.position.z - 10f;
        float totalDistance = endPositionZ - startPositionZ;

        if (Mathf.Approximately(totalDistance, 0f))
            return;

        inverseTotalDistance = 1f / totalDistance;
        tooltipPanel.SetActive(false);
        isReady = true;
    }

    #endregion

    #region Intro Sequence

    async void StartIntroSequence()
    {
        UpdateUIInfo();

        targetTextPosition = stageInfoText.rectTransform.anchoredPosition;
        stageInfoText.rectTransform.anchoredPosition = Vector2.zero;
        stageInfoText.transform.localScale = Vector3.one * 2f;

        if (darkScreen != null)
        {
            darkScreen.gameObject.SetActive(true);
            darkScreen.color = new Color(0f, 0f, 0f, 0.9f);
        }

        await Tween.Delay(1f, useUnscaledTime: true);

        _ = Tween.PunchScale(stageInfoText.transform, strength: Vector3.one * 0.5f, duration: 1.7f, frequency: 3, useUnscaledTime: true);

        await Tween.Delay(3f, useUnscaledTime: true);

        _ = Tween.UIAnchoredPosition(stageInfoText.rectTransform, targetTextPosition, 1f, Ease.InOutQuad, useUnscaledTime: true);
        _ = Tween.Scale(stageInfoText.transform, Vector3.one, 1f, Ease.InOutQuad, useUnscaledTime: true);

        if (darkScreen != null)
            _ = Tween.Alpha(darkScreen, 0f, 1f, Ease.InOutQuad, useUnscaledTime: true);

        await Tween.Delay(1.3f, useUnscaledTime: true);

        if (darkScreen != null)
            darkScreen.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    #endregion

    #region Core Logic

    void Update()
    {
        if (!isReady || cameraTargetTransform == null)
            return;

        float currentDistance = cameraTargetTransform.position.z - startPositionZ;
        float targetValue = Mathf.Clamp01(currentDistance * inverseTotalDistance);

        if (Mathf.Abs(lastSliderValue - targetValue) < 0.001f)
            return;

        lastSliderValue = targetValue;
        progressSlider.value = targetValue;
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