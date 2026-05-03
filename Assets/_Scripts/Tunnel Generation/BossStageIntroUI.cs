using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;

public class BossStageIntroUI : MonoBehaviour
{
    [SerializeField] private MapState mapState;
    [SerializeField] private string bossObjective = "Destroy all generators and catch the engineer";

    [SerializeField] private TextMeshProUGUI stageInfoText;
    [SerializeField] private Image darkScreen;

    private Vector2 targetTextPosition;

    private void Awake() => Time.timeScale = 0f;

    private void Start() => StartIntroSequence();

    private async void StartIntroSequence()
    {
        SetupUI();

        await Tween.Delay(1f, useUnscaledTime: true);

        _ = Tween.PunchScale(stageInfoText.transform, Vector3.one * 0.2f, 1.7f, 2, useUnscaledTime: true);

        await Tween.Delay(4f, useUnscaledTime: true);

        _ = Tween.UIAnchoredPosition(stageInfoText.rectTransform, targetTextPosition, 1f, Ease.InOutQuad, useUnscaledTime: true);
        _ = Tween.Scale(stageInfoText.transform, Vector3.one, 1f, Ease.InOutQuad, useUnscaledTime: true);

        if (darkScreen != null)
            _ = Tween.Alpha(darkScreen, 0f, 1f, Ease.InOutQuad, useUnscaledTime: true);

        await Tween.Delay(1.5f, useUnscaledTime: true);

        if (darkScreen != null)
            darkScreen.gameObject.SetActive(false);

        Time.timeScale = 1f;
    }

    private void SetupUI()
    {
        string stageNumber = mapState != null ? (mapState.CurrentRow + 1).ToString() : "";

        stageInfoText.text = $"BOSS STAGE: <color=#FFD700>{bossObjective}</color>";

        targetTextPosition = stageInfoText.rectTransform.anchoredPosition;

        stageInfoText.rectTransform.anchoredPosition = Vector2.zero;
        stageInfoText.transform.localScale = Vector3.one * 1.5f;

        if (darkScreen == null)
            return;

        darkScreen.gameObject.SetActive(true);
        darkScreen.color = new Color(0f, 0f, 0f, 0.9f);
    }
}