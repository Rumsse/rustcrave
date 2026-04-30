using UnityEngine;
using UnityEngine.UIElements;

public class InfoTooltipController
{
    readonly VisualElement container;
    bool isVisible;

    public InfoTooltipController(VisualElement tooltipElement)
    {
        container = tooltipElement;
        container.style.position = Position.Absolute;
        container.pickingMode = PickingMode.Ignore;
        container.style.display = DisplayStyle.None;

        SetAllChildrenNonPicking(container);
    }

    void SetAllChildrenNonPicking(VisualElement element)
    {
        element.pickingMode = PickingMode.Ignore;

        foreach (var child in element.Children())
            SetAllChildrenNonPicking(child);
    }

    public void Show(Vector2 position)
    {
        UpdatePosition(position);
        container.style.display = DisplayStyle.Flex;
        isVisible = true;
    }

    public void Hide()
    {
        container.style.display = DisplayStyle.None;
        isVisible = false;
    }

    public void UpdatePosition(Vector2 position)
    {
        if (!isVisible)
            return;

        Vector2 localPos = container.parent != null ? container.parent.WorldToLocal(position) : position;

        container.style.left = localPos.x - 435;
        container.style.top = localPos.y - 75;
    }
}