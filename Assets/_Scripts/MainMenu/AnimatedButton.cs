using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimatedTextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private float tweenDuration = 0.15f;

    private Vector3 defaultScale;
    private Color defaultColor;

    private void Awake()
    {
        defaultScale = transform.localScale;

        if (targetText == null)
            return;

        defaultColor = targetText.color;
    }

    public void OnPointerEnter(PointerEventData eventData) => Animate(hoverScale, hoverColor);

    public void OnPointerExit(PointerEventData eventData) => Animate(defaultScale, defaultColor);

    private void Animate(Vector3 targetScale, Color targetColor)
    {
        Tween.Scale(transform, targetScale, tweenDuration, Ease.OutQuad, useUnscaledTime: true);

        if (targetText == null)
            return;

        Tween.Color(targetText, targetColor, tweenDuration, Ease.OutQuad, useUnscaledTime: true);
    }
}