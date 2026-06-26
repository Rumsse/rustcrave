using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using PrimeTween;

[Serializable]
public class HighlightTarget
{
    public string identifier;
    public float borderThickness = 8f;
    public float padding = 3f;
    public float cornerRadius = 0f;
}

[Serializable]
public struct TutorialHighlightRule
{
    public TutorialTaskType taskType;
    public List<HighlightTarget> targets;
    public List<string> allowedInteractionElements;
}

public class TutorialUIHighlighter : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string highlightClassName = "tutorial-highlight";
    [SerializeField] private List<TutorialHighlightRule> highlightRules;

    private readonly List<VisualElement> currentHighlightedElements = new();
    private readonly List<VisualElement> activeOverlays = new();
    private readonly Dictionary<VisualElement, StyleEnum<Overflow>> originalOverflows = new();

    private TutorialHighlightRule? activeRule;
    private HighlightTarget currentTargetData;
    private Tween pulseTween;

    #region Unity Lifecycle

    private void Start()
    {
        if (TutorialTaskVerifier.Instance == null)
        {
            Debug.LogError("[TutorialUIHighlighter] TutorialTaskVerifier is NULL in Start!");
            return;
        }

        TutorialTaskVerifier.Instance.OnTaskStarted += HandleTaskStarted;
        TutorialTaskVerifier.Instance.OnTaskEnded += HandleTaskEnded;

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.RegisterCallback<PointerDownEvent>(BlockUnauthorizedInteraction, TrickleDown.TrickleDown);
            uiDocument.rootVisualElement.RegisterCallback<PointerUpEvent>(BlockUnauthorizedInteraction, TrickleDown.TrickleDown);
            uiDocument.rootVisualElement.RegisterCallback<ClickEvent>(BlockUnauthorizedInteraction, TrickleDown.TrickleDown);
        }

        if (TutorialTaskVerifier.Instance.CurrentTask != TutorialTaskType.None)
            HandleTaskStarted(TutorialTaskVerifier.Instance.CurrentTask);
    }

    private void OnDestroy()
    {
        if (TutorialTaskVerifier.Instance != null)
        {
            TutorialTaskVerifier.Instance.OnTaskStarted -= HandleTaskStarted;
            TutorialTaskVerifier.Instance.OnTaskEnded -= HandleTaskEnded;
        }

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.UnregisterCallback<PointerDownEvent>(BlockUnauthorizedInteraction, TrickleDown.TrickleDown);
            uiDocument.rootVisualElement.UnregisterCallback<PointerUpEvent>(BlockUnauthorizedInteraction, TrickleDown.TrickleDown);
            uiDocument.rootVisualElement.UnregisterCallback<ClickEvent>(BlockUnauthorizedInteraction, TrickleDown.TrickleDown);
        }

        RemoveCurrentHighlight();
    }

    private void Update()
    {
        if (!activeRule.HasValue || uiDocument == null || uiDocument.rootVisualElement == null)
            return;

        var result = FindActiveVisibleElements(activeRule.Value.targets);

        if (AreListsEqual(result.elements, currentHighlightedElements))
            return;

        SwitchHighlight(result.elements, result.targetData);
    }

    #endregion

    #region Interaction Blocking

    private void BlockUnauthorizedInteraction<T>(T evt) where T : EventBase
    {
        if (TutorialTaskVerifier.Instance == null || TutorialTaskVerifier.Instance.CurrentTask == TutorialTaskType.None)
            return;

        var targetElement = evt.target as VisualElement;

        if (targetElement == null)
            return;

        foreach (var el in currentHighlightedElements)
        {
            if (targetElement == el || el.Contains(targetElement))
                return;
        }

        if (activeRule.HasValue && activeRule.Value.allowedInteractionElements != null)
        {
            foreach (var elementName in activeRule.Value.allowedInteractionElements)
            {
                var allowedElements = FindAllElementsByIdentifier(elementName);

                foreach (var allowedElement in allowedElements)
                {
                    if (targetElement == allowedElement || allowedElement.Contains(targetElement))
                        return;
                }
            }
        }

        evt.StopImmediatePropagation();
        evt.PreventDefault();
    }

    #endregion

    #region Logic & Highlighting

    private void HandleTaskStarted(TutorialTaskType task)
    {
        RemoveCurrentHighlight();
        activeRule = null;

        foreach (var rule in highlightRules)
        {
            if (rule.taskType != task)
                continue;

            activeRule = rule;
            break;
        }
    }

    private void HandleTaskEnded(TutorialTaskType task)
    {
        RemoveCurrentHighlight();
        activeRule = null;
    }

    private void SwitchHighlight(List<VisualElement> newElements, HighlightTarget targetData)
    {
        RemoveCurrentHighlight();

        if (newElements == null || newElements.Count == 0 || targetData == null)
            return;

        currentTargetData = targetData;
        currentHighlightedElements.AddRange(newElements);

        float offset = targetData.borderThickness + targetData.padding;
        float outerRadius = targetData.cornerRadius > 0f ? targetData.cornerRadius + offset : 0f;

        foreach (var el in currentHighlightedElements)
        {
            originalOverflows[el] = el.style.overflow;
            el.style.overflow = Overflow.Visible;

            var overlay = new VisualElement();
            overlay.pickingMode = PickingMode.Ignore;
            overlay.AddToClassList(highlightClassName);

            overlay.style.position = Position.Absolute;
            overlay.style.top = -offset;
            overlay.style.bottom = -offset;
            overlay.style.left = -offset;
            overlay.style.right = -offset;

            overlay.style.borderTopWidth = targetData.borderThickness;
            overlay.style.borderBottomWidth = targetData.borderThickness;
            overlay.style.borderLeftWidth = targetData.borderThickness;
            overlay.style.borderRightWidth = targetData.borderThickness;

            if (targetData.cornerRadius > 0f)
            {
                overlay.style.borderTopLeftRadius = outerRadius;
                overlay.style.borderTopRightRadius = outerRadius;
                overlay.style.borderBottomLeftRadius = outerRadius;
                overlay.style.borderBottomRightRadius = outerRadius;
            }

            el.Add(overlay);
            activeOverlays.Add(overlay);

            el.RegisterCallback<GeometryChangedEvent>(SyncHighlightShape);
        }

        pulseTween = Tween.Custom(
            startValue: 1f,
            endValue: 1.05f,
            duration: 0.6f,
            onValueChange: val =>
            {
                foreach (var el in currentHighlightedElements)
                {
                    if (el != null)
                        el.style.scale = new StyleScale(new Vector2(val, val));
                }
            },
            cycles: -1,
            cycleMode: CycleMode.Yoyo,
            ease: Ease.InOutSine
        );
    }

    private void SyncHighlightShape(GeometryChangedEvent evt)
    {
        var parent = evt.currentTarget as VisualElement;

        if (parent == null || currentTargetData == null)
            return;

        var overlay = parent.Q(className: highlightClassName);

        if (overlay == null)
            return;

        if (currentTargetData.cornerRadius > 0f)
            return;

        var style = parent.resolvedStyle;
        float offset = currentTargetData.borderThickness + currentTargetData.padding;

        overlay.style.borderTopLeftRadius = new StyleLength(style.borderTopLeftRadius + offset);
        overlay.style.borderTopRightRadius = new StyleLength(style.borderTopRightRadius + offset);
        overlay.style.borderBottomLeftRadius = new StyleLength(style.borderBottomLeftRadius + offset);
        overlay.style.borderBottomRightRadius = new StyleLength(style.borderBottomRightRadius + offset);
    }

    private void RemoveCurrentHighlight()
    {
        if (pulseTween.isAlive)
            pulseTween.Stop();

        foreach (var el in currentHighlightedElements)
        {
            if (el == null)
                continue;

            el.style.scale = new StyleScale(new Vector2(1f, 1f));

            if (originalOverflows.TryGetValue(el, out var overflow))
                el.style.overflow = overflow;

            el.UnregisterCallback<GeometryChangedEvent>(SyncHighlightShape);
        }

        foreach (var overlay in activeOverlays)
        {
            if (overlay != null)
                overlay.RemoveFromHierarchy();
        }

        currentHighlightedElements.Clear();
        activeOverlays.Clear();
        originalOverflows.Clear();
        currentTargetData = null;
    }

    private List<VisualElement> FindAllElementsByIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier) || uiDocument == null || uiDocument.rootVisualElement == null)
            return new List<VisualElement>();

        return identifier.StartsWith(".")
            ? uiDocument.rootVisualElement.Query(className: identifier.Substring(1)).ToList()
            : uiDocument.rootVisualElement.Query(identifier).ToList();
    }

    private (List<VisualElement> elements, HighlightTarget targetData) FindActiveVisibleElements(List<HighlightTarget> targets)
    {
        if (targets == null || targets.Count == 0)
            return (new List<VisualElement>(), null);

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            var elements = FindAllElementsByIdentifier(targets[i].identifier);
            var visibleElements = new List<VisualElement>();

            foreach (var el in elements)
            {
                if (IsElementVisible(el))
                    visibleElements.Add(el);
            }

            if (visibleElements.Count > 0)
                return (visibleElements, targets[i]);
        }

        return (new List<VisualElement>(), null);
    }

    private bool IsElementVisible(VisualElement element)
    {
        if (element == null)
            return false;

        var current = element;

        while (current != null)
        {
            if (current.style.display == DisplayStyle.None)
                return false;

            if (current.resolvedStyle.display == DisplayStyle.None)
                return false;

            current = current.parent;
        }

        return true;
    }

    private bool AreListsEqual(List<VisualElement> list1, List<VisualElement> list2)
    {
        if (list1.Count != list2.Count)
            return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i])
                return false;
        }

        return true;
    }

    #endregion
}