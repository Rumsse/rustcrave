using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosePathController : MonoBehaviour
{
    #region Refs
    
    [System.Serializable]
    public class NodeUIComponents
    {
        public Button button;
        public VisualElement icon;
    }

    [SerializeField] MapState mapState;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] GlobalInventorySO globalInventory;
    [SerializeField] OreSO pulsite;
    [SerializeField] VisualTreeAsset nodeButtonAsset;
    [SerializeField] VisualTreeAsset tooltipAsset;

    [SerializeField] string mainGameScene = "new Tunel Generation Rumsse";
    [SerializeField] string mainBossScene = "boss map";

    [SerializeField] Sprite unknownIcon;
    [SerializeField] Sprite bossIcon;

    [SerializeField] float lineNodePadding = 20f;
    private float spreadH = 20f;
    private float spreadV = 20f;

    [SerializeField] bool debugMode = true;

    readonly Dictionary<string, NodeUIComponents> nodeUIMap = new();
    VisualElement nodesLayer;
    VisualElement linesLayer;
    PathNodeTooltipController tooltipController;

    #endregion

    #region Initialization

    public void Initialize(VisualElement panelRoot, VisualElement tooltipLayer)
    {
        if (mapState.nodes.Count == 0)
            mapState.Initialize();

        nodesLayer = panelRoot.Q<VisualElement>("nodes-layer");
        linesLayer = panelRoot.Q<VisualElement>("path-lines-layer");

        if (tooltipController == null)
            tooltipController = new PathNodeTooltipController(tooltipLayer, tooltipAsset);

        GenerateUI();
        linesLayer.generateVisualContent += OnGenerateLines;

        UpdateButtonStates();
    }

    void GenerateUI()
    {
        nodesLayer.Clear();
        nodeUIMap.Clear();

        float startRows = mapState.TotalRows;
        float offsetV = 50f - (((startRows - 1f) / 2f) * spreadV);

        float totalColumns = mapState.TotalColumns;
        float offsetH = 50f - (((totalColumns - 1f) / 2f) * spreadH);

        foreach (var node in mapState.nodes)
        {
            var btn = nodeButtonAsset.Instantiate().Q<Button>();
            if (btn == null) { Debug.LogError("Failed to find button"); continue; }

            VisualElement icon = btn.Q<VisualElement>("node-icon");
            if (icon == null) { Debug.LogWarning("node-icon not found in button UXML"); }

            var uiComponents = new NodeUIComponents { button = btn, icon = icon };
            nodeUIMap[node.id] = uiComponents;

            SetupButtonPosition(btn, node, offsetH, offsetV);
            RegisterNodeEvents(btn, node);
            nodesLayer.Add(btn);

            if (debugMode)
                SetupDebugButton(btn, node);

            if (icon != null) { icon.style.backgroundImage = new StyleBackground(unknownIcon); }
        }

        nodesLayer.RegisterCallback<GeometryChangedEvent>(evt => linesLayer.MarkDirtyRepaint());
    }

    void SetupButtonPosition(Button btn, PathNodeData node, float offsetH, float offsetV)
    {
        btn.style.position = Position.Absolute;

        float left = node.columnPosition * spreadH + offsetH;
        float bottom = node.row * spreadV + offsetV;

        btn.style.left = new StyleLength(Length.Percent(left));
        btn.style.bottom = new StyleLength(Length.Percent(bottom));
        btn.style.translate = new StyleTranslate(new Translate(Length.Percent(-50f), Length.Percent(50f)));
    }

    #endregion

    #region Main Logic

    void UpdateButtonStates()
    {
        var available = mapState.GetAvailableNodes();

        foreach (var node in mapState.nodes)
        {
            if (!nodeUIMap.TryGetValue(node.id, out var ui))
                continue;

            Button btn = ui.button;
            VisualElement iconElement = ui.icon;

            btn.RemoveFromClassList("node-current");
            btn.RemoveFromClassList("node-visited");
            btn.RemoveFromClassList("node-available");
            btn.RemoveFromClassList("node-locked");

            bool isBoss = node.row == mapState.TotalRows - 1;
            bool isCurrent = node.id == mapState.currentNodeId;
            bool isVisited = mapState.IsVisited(node);
            bool isScanned = mapState.IsNodeScanned(node);
            bool isAvailable = available.Any(n => n.id == node.id);
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
                btn.AddToClassList("node-current");
            else if (isVisited)
                btn.AddToClassList("node-visited");
            else if (isAvailable)
                btn.AddToClassList("node-available");
            else
                btn.AddToClassList("node-locked");
        }
    }

    void OnNodeClicked(PathNodeData node)
    {
        var available = mapState.GetAvailableNodes();
        if (!available.Any(n => n.id == node.id))
            return;

        mapState.MoveToNode(node);
        activeModifier.Set(mapState.GetModifier(node));

        UpdateButtonStates();

        string scene = node.row == mapState.TotalRows - 1 ? mainBossScene : mainGameScene;
        SceneManager.LoadScene(scene);
    }

    void OnNodeRightClicked(PathNodeData node)
    {
        if (mapState.IsNodeScanned(node))
            return;

        bool isVisitedOrCurrent = mapState.IsVisited(node) || node.id == mapState.currentNodeId;
        if (isVisitedOrCurrent)
            return;

        if (globalInventory.Pulsite < 1)
            return;

        globalInventory.RemoveItem(pulsite, 1);
        mapState.ScanNode(node);

        UpdateButtonStates();
        tooltipController.RefreshContent();
    }

    #endregion

    #region Helpers & Lines

    void RegisterNodeEvents(Button btn, PathNodeData node)
    {
        btn.clicked += () => OnNodeClicked(node);
        btn.RegisterCallback<PointerEnterEvent>(evt => tooltipController.Show(node, mapState, evt.position));
        btn.RegisterCallback<PointerLeaveEvent>(evt => tooltipController.Hide());
        btn.RegisterCallback<PointerMoveEvent>(evt => tooltipController.UpdatePosition(evt.position));
        btn.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button == 1)
                OnNodeRightClicked(node);
        });
    }

    void OnGenerateLines(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        painter.strokeColor = new Color(1f, 1f, 1f, 0.35f);
        painter.lineWidth = 3f;

        var drawnLines = new HashSet<string>();

        foreach (var node in mapState.nodes)
        {
            if (node.connectedToNodes == null)
                continue;

            if (!nodeUIMap.TryGetValue(node.id, out var ui) || ui.button.worldBound.width == 0)
                continue;

            Vector2 startPos = GetBtnCenter(ui.button);

            foreach (var targetId in node.connectedToNodes)
            {
                string lineKey = string.Compare(node.id, targetId) < 0 ? $"{node.id}_{targetId}" : $"{targetId}_{node.id}";
                if (drawnLines.Contains(lineKey))
                    continue;

                drawnLines.Add(lineKey);

                if (!nodeUIMap.TryGetValue(targetId, out var endUI) || endUI.button.worldBound.width == 0)
                    continue;

                Vector2 endPos = GetBtnCenter(endUI.button);
                Vector2 direction = (endPos - startPos).normalized;

                if (Vector2.Distance(startPos, endPos) <= lineNodePadding * 2.5f)
                    continue;

                painter.BeginPath();
                painter.MoveTo(startPos + direction * lineNodePadding);
                painter.LineTo(endPos - direction * lineNodePadding);
                painter.Stroke();
            }
        }
    }

    Vector2 GetBtnCenter(Button btn) => linesLayer.WorldToLocal(btn.worldBound.center);

    void SetupDebugButton(Button btn, PathNodeData node)
    {
        var modifier = mapState.GetModifier(node);
        btn.tooltip = $"[R: {node.row}, C: {node.columnPosition:F1}] {modifier.DisplayName}";

        var debugLabel = new Label(modifier.DisplayName);
        debugLabel.style.position = Position.Absolute;
        debugLabel.style.bottom = -15f;
        debugLabel.style.width = Length.Percent(200f);
        debugLabel.style.left = Length.Percent(-50f);
        debugLabel.style.alignSelf = Align.Center;
        debugLabel.style.fontSize = 9f;
        debugLabel.style.color = new Color(1f, 1f, 0.4f, 0.8f);
        debugLabel.style.whiteSpace = WhiteSpace.NoWrap;
        btn.Add(debugLabel);
    }

    #endregion
}