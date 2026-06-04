using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosePathController : MonoBehaviour, IPanelController
{
    #region Refs

    [System.Serializable]
    public class NodeUIComponents
    {
        public Button button;
        public VisualElement icon;
    }

    public static event System.Action OnAnyPathNodeEntered;

    [SerializeField] MapState mapState;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] GlobalInventorySO globalInventory;
    [SerializeField] OreSO pulsiteOre;
    [SerializeField] VisualTreeAsset nodeButtonAsset;
    [SerializeField] VisualTreeAsset tooltipAsset;

    [SerializeField] string mainGameScene = "new Tunel Generation Rumsse";
    [SerializeField] string mainBossScene = "boss map";

    [SerializeField] Sprite unknownIcon;
    [SerializeField] Sprite bossIcon;

    [SerializeField] float lineNodePadding = 20f;
    [SerializeField] float horizontalSpread = 20f;
    [SerializeField] float verticalSpread = 20f;
    [SerializeField] bool debugMode = true;

    readonly Dictionary<string, NodeUIComponents> nodeUIMap = new();
    VisualElement nodesLayer;
    VisualElement linesLayer;
    PathNodeTooltipController tooltipController;

    #endregion

    #region Initialization

    public void Initialize(VisualElement panel, VisualElement contextLayer = null)
    {
        if (mapState.Nodes.Count == 0)
            mapState.Initialize();

        nodesLayer = panel.Q<VisualElement>("nodes-layer");
        linesLayer = panel.Q<VisualElement>("path-lines-layer");

        if (tooltipController == null)
            tooltipController = new PathNodeTooltipController(contextLayer, tooltipAsset);

        GenerateUI();
        linesLayer.generateVisualContent += OnGenerateLines;

        UpdateButtonStates();
    }

    public void NotifyPanelOpened() { }

    public void NotifyPanelClosed() { }

    void GenerateUI()
    {
        nodesLayer.Clear();
        nodeUIMap.Clear();

        float startRows = mapState.TotalRows;
        float verticalOffset = 50f - (((startRows - 1f) / 2f) * verticalSpread);

        float totalColumns = mapState.TotalColumns;
        float horizontalOffset = 50f - (((totalColumns - 1f) / 2f) * horizontalSpread);

        foreach (var node in mapState.Nodes)
        {
            var nodeButton = nodeButtonAsset.Instantiate().Q<Button>();
            if (nodeButton == null)
            {
                Debug.LogError("Failed to find button");
                continue;
            }

            VisualElement icon = nodeButton.Q<VisualElement>("node-icon");
            if (icon == null)
                Debug.LogWarning("node-icon not found in button UXML");

            var uiComponents = new NodeUIComponents { button = nodeButton, icon = icon };
            nodeUIMap[node.Id] = uiComponents;

            SetupButtonPosition(nodeButton, node, horizontalOffset, verticalOffset);
            RegisterNodeEvents(nodeButton, node);
            nodesLayer.Add(nodeButton);

            if (debugMode)
                SetupDebugButton(nodeButton, node);

            if (icon != null)
                icon.style.backgroundImage = new StyleBackground(unknownIcon);
        }

        nodesLayer.RegisterCallback<GeometryChangedEvent>(pointerEvent => linesLayer.MarkDirtyRepaint());
    }

    void SetupButtonPosition(Button nodeButton, PathNodeData node, float horizontalOffset, float verticalOffset)
    {
        nodeButton.style.position = Position.Absolute;

        float left = node.ColumnPosition * horizontalSpread + horizontalOffset;
        float bottom = node.Row * verticalSpread + verticalOffset;

        nodeButton.style.left = new StyleLength(Length.Percent(left));
        nodeButton.style.bottom = new StyleLength(Length.Percent(bottom));
        nodeButton.style.translate = new StyleTranslate(new Translate(Length.Percent(-50f), Length.Percent(50f)));
    }

    #endregion

    #region Main Logic

    void UpdateButtonStates()
    {
        var availableNodes = mapState.GetAvailableNodes();

        foreach (var node in mapState.Nodes)
        {
            if (!nodeUIMap.TryGetValue(node.Id, out var uiComponents))
                continue;

            Button nodeButton = uiComponents.button;
            VisualElement iconElement = uiComponents.icon;

            nodeButton.RemoveFromClassList("node-current");
            nodeButton.RemoveFromClassList("node-visited");
            nodeButton.RemoveFromClassList("node-available");
            nodeButton.RemoveFromClassList("node-locked");

            bool isBoss = node.Row == mapState.TotalRows - 1;
            bool isCurrent = node.Id == mapState.CurrentNodeId;
            bool isVisited = mapState.IsVisited(node);
            bool isScanned = mapState.IsNodeScanned(node);
            bool isAvailable = availableNodes.Any(n => n.Id == node.Id);
            bool isIdentityKnown = isCurrent || isVisited || isScanned;

            Sprite iconToShow = unknownIcon;

            if (isBoss)
            {
                iconToShow = bossIcon;
            }
            else if (isIdentityKnown)
            {
                PathModifierData modifier = mapState.GetModifier(node);
                iconToShow = modifier.Icon;
            }

            if (iconElement != null)
            {
                if (iconToShow != null)
                {
                    iconElement.style.backgroundImage = new StyleBackground(iconToShow);
                    iconElement.style.display = DisplayStyle.Flex;
                }
                else
                {
                    iconElement.style.backgroundImage = null;
                    iconElement.style.display = DisplayStyle.None;
                }
            }

            if (isCurrent)
                nodeButton.AddToClassList("node-current");
            else if (isVisited)
                nodeButton.AddToClassList("node-visited");
            else if (isAvailable)
                nodeButton.AddToClassList("node-available");
            else
                nodeButton.AddToClassList("node-locked");
        }
    }

    async void OnNodeClicked(PathNodeData node)
    {
        var availableNodes = mapState.GetAvailableNodes();
        if (!availableNodes.Any(n => n.Id == node.Id))
            return;

        bool isCompletingTutorialTask = false;

        if (TutorialTaskVerifier.Instance != null && TutorialTaskVerifier.Instance.CurrentTask == TutorialTaskType.ScanPathAndGo)
        {
            if (!mapState.IsNodeScanned(node))
            {
                Debug.LogWarning("[Tutorial] Node must be scanned first!");
                return;
            }

            isCompletingTutorialTask = true;
        }

        mapState.MoveToNode(node);
        activeModifier.Set(mapState.GetModifier(node));

        UpdateButtonStates();

        OnAnyPathNodeEntered?.Invoke();

        if (isCompletingTutorialTask)
            return;

        string sceneToLoad = node.Row == mapState.TotalRows - 1 ? mainBossScene : mainGameScene;

        if (SceneTransitionManager.Instance == null)
        {
            SceneManager.LoadScene(sceneToLoad);
            return;
        }

        await SceneTransitionManager.Instance.WipeToScene(sceneToLoad, true);
    }

    void OnNodeRightClicked(PathNodeData node)
    {
        if (mapState.IsNodeScanned(node))
            return;

        bool isVisitedOrCurrent = mapState.IsVisited(node) || node.Id == mapState.CurrentNodeId;
        if (isVisitedOrCurrent)
            return;

        if (globalInventory.Pulsite < 1)
            return;

        globalInventory.RemoveItem(pulsiteOre, 1);
        mapState.ScanNode(node);

        UpdateButtonStates();
        tooltipController.RefreshContent();
    }

    #endregion

    #region Helpers & Lines

    void RegisterNodeEvents(Button nodeButton, PathNodeData node)
    {
        nodeButton.clicked += () => OnNodeClicked(node);
        nodeButton.RegisterCallback<PointerEnterEvent>(pointerEvent => tooltipController.Show(node, mapState, pointerEvent.position));
        nodeButton.RegisterCallback<PointerLeaveEvent>(pointerEvent => tooltipController.Hide());
        nodeButton.RegisterCallback<PointerMoveEvent>(pointerEvent => tooltipController.UpdatePosition(pointerEvent.position));
        nodeButton.RegisterCallback<PointerDownEvent>(pointerEvent =>
        {
            if (pointerEvent.button == 1)
                OnNodeRightClicked(node);
        });
    }

    void OnGenerateLines(MeshGenerationContext meshContext)
    {
        var painter = meshContext.painter2D;
        painter.strokeColor = new Color(1f, 1f, 1f, 0.35f);
        painter.lineWidth = 3f;

        var drawnLines = new HashSet<string>();

        foreach (var node in mapState.Nodes)
        {
            if (node.ConnectedNodeIds == null)
                continue;

            if (!nodeUIMap.TryGetValue(node.Id, out var uiComponents) || uiComponents.button.worldBound.width == 0)
                continue;

            Vector2 startPosition = GetButtonCenter(uiComponents.button);

            foreach (var targetId in node.ConnectedNodeIds)
            {
                string lineKey = string.Compare(node.Id, targetId) < 0 ? $"{node.Id}_{targetId}" : $"{targetId}_{node.Id}";
                if (drawnLines.Contains(lineKey))
                    continue;

                drawnLines.Add(lineKey);

                if (!nodeUIMap.TryGetValue(targetId, out var endUIComponents) || endUIComponents.button.worldBound.width == 0)
                    continue;

                Vector2 endPosition = GetButtonCenter(endUIComponents.button);
                Vector2 direction = (endPosition - startPosition).normalized;

                if (Vector2.Distance(startPosition, endPosition) <= lineNodePadding * 2.5f)
                    continue;

                painter.BeginPath();
                painter.MoveTo(startPosition + direction * lineNodePadding);
                painter.LineTo(endPosition - direction * lineNodePadding);
                painter.Stroke();
            }
        }
    }

    Vector2 GetButtonCenter(Button nodeButton) => linesLayer.WorldToLocal(nodeButton.worldBound.center);

    void SetupDebugButton(Button nodeButton, PathNodeData node)
    {
        var modifier = mapState.GetModifier(node);
        nodeButton.tooltip = $"[R: {node.Row}, C: {node.ColumnPosition:F1}] {modifier.DisplayName}";

        var debugLabel = new Label(modifier.DisplayName);
        debugLabel.style.position = Position.Absolute;
        debugLabel.style.bottom = -15f;
        debugLabel.style.width = Length.Percent(200f);
        debugLabel.style.left = Length.Percent(-50f);
        debugLabel.style.alignSelf = Align.Center;
        debugLabel.style.fontSize = 9f;
        debugLabel.style.color = new Color(1f, 1f, 0.4f, 0.8f);
        debugLabel.style.whiteSpace = WhiteSpace.NoWrap;

        nodeButton.Add(debugLabel);
    }

    #endregion
}