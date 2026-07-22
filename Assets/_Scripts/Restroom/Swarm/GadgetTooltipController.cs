using UnityEngine;
using UnityEngine.UIElements;

public class GadgetTooltipController
{
    readonly VisualElement container;
    readonly Label nameLabel;
    readonly Label descriptionLabel;
    bool isVisible;

    public GadgetTooltipController(VisualElement tooltipElement)
    {
        container = tooltipElement;
        container.style.position = Position.Absolute;
        container.pickingMode = PickingMode.Ignore;
        container.style.display = DisplayStyle.None;

        nameLabel = container.Q<Label>("gadget-name");
        descriptionLabel = container.Q<Label>("gadget-description");

        SetAllChildrenNonPicking(container);
    }

    void SetAllChildrenNonPicking(VisualElement element)
    {
        element.pickingMode = PickingMode.Ignore;

        foreach (var child in element.Children())
            SetAllChildrenNonPicking(child);
    }

    public void Show(GadgetSO gadget, Vector2 position)
    {
        if (gadget == null)
            return;

        if (nameLabel != null)
            nameLabel.text = gadget.name;

        if (descriptionLabel != null)
            descriptionLabel.text = gadget.description;

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

        container.style.left = localPos.x + -72;
        container.style.top = localPos.y + 58;
    }
}