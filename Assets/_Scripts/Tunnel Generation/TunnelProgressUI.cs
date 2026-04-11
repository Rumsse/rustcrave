using System.Collections;
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
    float endPositionZ;
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

    void Start() => StartCoroutine(IntroSequenceRoutine());

    void SetupTracking()
    {
        var segments = tunnelGenerator.GetSpawnedSegments();

        if (segments.Count == 0)
            return;

        startPositionZ = startPos.transform.position.z;
        endPositionZ = segments[^1].transform.position.z - 10f;

        tooltipPanel.SetActive(false);
        isReady = true;
    }

    #endregion

    #region Intro Sequence

    IEnumerator IntroSequenceRoutine()
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

        yield return new WaitForSecondsRealtime(1f);

        Tween.PunchScale(stageInfoText.transform, strength: Vector3.one * 0.5f, duration: 1.7f, frequency: 3, useUnscaledTime: true);

        yield return new WaitForSecondsRealtime(3f);

        Tween.UIAnchoredPosition(stageInfoText.rectTransform, targetTextPosition, 1f, Ease.InOutQuad, useUnscaledTime: true);
        Tween.Scale(stageInfoText.transform, Vector3.one, 1f, Ease.InOutQuad, useUnscaledTime: true);

        if (darkScreen != null)
            Tween.Alpha(darkScreen, 0f, 1f, Ease.InOutQuad, useUnscaledTime: true);

        yield return new WaitForSecondsRealtime(1.3f);

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