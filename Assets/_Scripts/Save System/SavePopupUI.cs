using System;
using PrimeTween;
using TMPro;
using UnityEngine;

public class SavePopupUI : MonoBehaviour
{
    [SerializeField] TMP_Text popupText;
    [SerializeField] float showDuration = 0.3f;
    [SerializeField] float displayTime = 1.5f;
    [SerializeField] float hideDuration = 0.25f;
    [SerializeField] Vector3 popScale = new Vector3(1.1f, 1.1f, 1.1f);

    Sequence currentSequence;

    #region Unity Lifecycle

    void Awake()
    {
        if (popupText == null)
            return;

        popupText.transform.localScale = Vector3.zero;
        popupText.alpha = 0f;
    }

    void OnEnable() => SaveManager.OnGameSaved += ShowPopup;

    void OnDisable() => SaveManager.OnGameSaved -= ShowPopup;

    #endregion

    #region Animation Logic

    void ShowPopup()
    {
        if (popupText == null)
            return;

        currentSequence.Stop();

        popupText.transform.localScale = Vector3.zero;
        popupText.alpha = 0f;

        currentSequence = Sequence.Create()
            .Group(Tween.Scale(popupText.transform, popScale, showDuration, Ease.OutBack))
            .Group(Tween.Alpha(popupText, 1f, showDuration, Ease.OutQuad))
            .ChainDelay(displayTime)
            .Chain(Tween.Scale(popupText.transform, Vector3.zero, hideDuration, Ease.InBack))
            .Group(Tween.Alpha(popupText, 0f, hideDuration, Ease.InQuad));
    }

    #endregion
}