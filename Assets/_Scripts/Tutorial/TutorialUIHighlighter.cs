using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using PrimeTween;

[Serializable]
public struct TutorialHighlightRule
{
    public TutorialTaskType taskType;
    public List<string> elementNames;
}

public class TutorialUIHighlighter : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string highlightClassName = "tutorial-highlight";
    [SerializeField] private List<TutorialHighlightRule> highlightRules;

    private VisualElement currentHighlightedElement;
    private TutorialHighlightRule? activeRule;
    private Tween pulseTween;

    private void Start()
    {
        if (TutorialTaskVerifier.Instance == null)
        {
            Debug.LogError("[TutorialUIHighlighter] TutorialTaskVerifier is NULL in Start!");
            return;
        }

        TutorialTaskVerifier.Instance.OnTaskStarted += HandleTaskStarted;
        TutorialTaskVerifier.Instance.OnTaskEnded += HandleTaskEnded;

        if (TutorialTaskVerifier.Instance.CurrentTask != TutorialTaskType.None)
            HandleTaskStarted(TutorialTaskVerifier.Instance.CurrentTask);
    }

    private void OnDestroy()
    {
        if (TutorialTaskVerifier.Instance == null) return;

        TutorialTaskVerifier.Instance.OnTaskStarted -= HandleTaskStarted;
        TutorialTaskVerifier.Instance.OnTaskEnded -= HandleTaskEnded;

        RemoveCurrentHighlight();
    }

    private void Update()
    {
        if (!activeRule.HasValue || uiDocument == null || uiDocument.rootVisualElement == null) return;

        var targetElement = FindDeepestVisibleElement(activeRule.Value.elementNames);

        if (targetElement == currentHighlightedElement) return;

        SwitchHighlight(targetElement);
    }

    private void HandleTaskStarted(TutorialTaskType task)
    {
        RemoveCurrentHighlight();
        activeRule = null;

        foreach (var rule in highlightRules)
        {
            if (rule.taskType != task) continue;

            activeRule = rule;
            break;
        }
    }

    private void HandleTaskEnded(TutorialTaskType task)
    {
        RemoveCurrentHighlight();
        activeRule = null;
    }

    private void SwitchHighlight(VisualElement newElement)
    {
        RemoveCurrentHighlight();

        if (newElement == null) return;

        currentHighlightedElement = newElement;
        currentHighlightedElement.AddToClassList(highlightClassName);

        pulseTween = Tween.Custom(
            startValue: 1f,
            endValue: 1.05f,
            duration: 0.6f,
            onValueChange: val =>
            {
                if (currentHighlightedElement != null)
                    currentHighlightedElement.style.scale = new StyleScale(new Vector2(val, val));
            },
            cycles: -1,
            cycleMode: CycleMode.Yoyo,
            ease: Ease.InOutSine
        );
    }

    private void RemoveCurrentHighlight()
    {
        if (currentHighlightedElement == null) return;

        if (pulseTween.isAlive)
            pulseTween.Stop();

        currentHighlightedElement.style.scale = new StyleScale(new Vector2(1f, 1f));
        currentHighlightedElement.RemoveFromClassList(highlightClassName);
        currentHighlightedElement = null;
    }

    private VisualElement FindDeepestVisibleElement(List<string> names)
    {
        for (int i = names.Count - 1; i >= 0; i--)
        {
            var el = uiDocument.rootVisualElement.Q(names[i]);

            if (IsElementVisible(el)) return el;
        }

        return null;
    }

    private bool IsElementVisible(VisualElement element)
    {
        if (element == null) return false;

        var current = element;

        while (current != null)
        {
            if (current.style.display == DisplayStyle.None) return false;
            if (current.resolvedStyle.display == DisplayStyle.None) return false;

            current = current.parent;
        }

        return true;
    }
}