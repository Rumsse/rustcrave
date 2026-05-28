using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private Color hoverColor = Color.gray;
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
        Tween.Scale(transform, targetScale, tweenDuration, Ease.OutQuad);

        if (targetText == null)
            return;

        Tween.Color(targetText, targetColor, tweenDuration, Ease.OutQuad);
    }
}