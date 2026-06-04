using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PathNodeTooltipController
{
    readonly VisualElement container;
    readonly Label titleLabel;
    readonly Label actionLabel;
    readonly Label costLabel;

    PathNodeData currentNode;
    MapState currentMapState;
    bool isVisible;

    public PathNodeTooltipController(VisualElement tooltipLayer, VisualTreeAsset asset)
    {
        container = asset.Instantiate();
        container.style.position = Position.Absolute;
        container.pickingMode = PickingMode.Ignore;

        container.AddToClassList("animated-tooltip");
        container.AddToClassList("tooltip-hidden");

        SetAllChildrenNonPicking(container);

        titleLabel = container.Q<Label>("tooltip-title");
        actionLabel = container.Q<Label>("tooltip-action");
        costLabel = container.Q<Label>("tooltip-cost");

        tooltipLayer.Add(container);
    }

    void SetAllChildrenNonPicking(VisualElement element)
    {
        element.pickingMode = PickingMode.Ignore;

        foreach (var child in element.Children())
            SetAllChildrenNonPicking(child);
    }

    public void Show(PathNodeData node, MapState mapState, Vector2 position)
    {
        currentNode = node;
        currentMapState = mapState;

        UpdateContent();
        UpdatePosition(position);

        container.RemoveFromClassList("tooltip-hidden");
        isVisible = true;
    }

    public void Hide()
    {
        container.AddToClassList("tooltip-hidden");
        currentNode = null;
        currentMapState = null;
        isVisible = false;
    }

    public void UpdatePosition(Vector2 position)
    {
        if (!isVisible)
            return;

        container.style.left = position.x + 5;
        container.style.top = position.y - container.resolvedStyle.height - 150;
    }

    public void RefreshContent()
    {
        if (!isVisible)
            return;

        UpdateContent();
    }

    void UpdateContent()
    {
        bool isScanned = currentMapState.IsNodeScanned(currentNode);
        bool isVisited = currentMapState.IsVisited(currentNode);
        bool isCurrent = currentNode.Id == currentMapState.CurrentNodeId;
        bool isBoss = currentNode.Row == currentMapState.TotalRows - 1;
        bool isIdentityKnown = isScanned || isVisited || isCurrent;

        if (isBoss)
        {
            titleLabel.text = "BOSS";
            actionLabel.text = "FINAL ENCOUNTER";
            costLabel.text = "";
            return;
        }

        if (isIdentityKnown)
        {
            var modifier = currentMapState.GetModifier(currentNode);
            titleLabel.text = modifier.DisplayName.ToUpper();
            actionLabel.text = modifier.Description;
            costLabel.text = "";
            return;
        }

        titleLabel.text = "UNKNOWN";
        actionLabel.text = "PRESS RMB TO SCAN";
        costLabel.text = "COSTS 1 PULSITE";
    }
}