using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimatedTextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TMP_Text targetText;
    [SerializeField] Button buttonComponent;
    [SerializeField] Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] Color hoverColor = Color.white;
    [SerializeField] Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    [SerializeField] float tweenDuration = 0.15f;

    Vector3 defaultScale;
    Color defaultColor;
    bool isInteractable = true;

    void Awake()
    {
        defaultScale = transform.localScale;

        if (buttonComponent == null)
            buttonComponent = GetComponent<Button>();

        if (targetText != null)
            defaultColor = targetText.color;
    }

    public void SetInteractable(bool state)
    {
        isInteractable = state;

        if (buttonComponent != null)
            buttonComponent.interactable = state;

        if (targetText == null)
            return;

        Tween.StopAll(targetText);
        Tween.StopAll(transform);

        targetText.color = state ? defaultColor : disabledColor;
        transform.localScale = defaultScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isInteractable)
            Animate(hoverScale, hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isInteractable)
            Animate(defaultScale, defaultColor);
    }

    void Animate(Vector3 targetScale, Color targetColor)
    {
        Tween.Scale(transform, targetScale, tweenDuration, Ease.OutQuad, useUnscaledTime: true);

        if (targetText != null)
            Tween.Color(targetText, targetColor, tweenDuration, Ease.OutQuad, useUnscaledTime: true);
    }
}