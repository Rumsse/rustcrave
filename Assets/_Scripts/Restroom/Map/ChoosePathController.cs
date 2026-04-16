using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosePathController : MonoBehaviour
{
    [SerializeField] MapState mapState;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] VisualTreeAsset nodeButtonAsset;
    [SerializeField] VisualTreeAsset tooltipAsset;


    readonly Dictionary<string, Button> idToButtonMap = new();
    VisualElement nodesLayer;
    VisualElement linesLayer;
    PathNodeTooltipController tooltipController;

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
    }

    void GenerateUI()
    {
        nodesLayer.Clear();
        idToButtonMap.Clear();

        foreach (var node in mapState.nodes)
        {
            var btn = nodeButtonAsset.Instantiate().Q<Button>();
            idToButtonMap[node.id] = btn;

            SetupButtonPosition(btn, node);
            RegisterNodeEvents(btn, node);
            nodesLayer.Add(btn);
        }

        nodesLayer.RegisterCallback<GeometryChangedEvent>(evt => linesLayer.MarkDirtyRepaint());

        UpdateButtonStates();
    }

    void SetupButtonPosition(Button btn, PathNodeData node)
    {
        btn.style.position = Position.Absolute;

        btn.style.left = new StyleLength(Length.Percent(node.columnPosition * 20 + 10));
        btn.style.bottom = new StyleLength(Length.Percent(node.row * 15 + 10));
    }

    #endregion

    #region Lines Rendering

    void OnGenerateLines(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        painter.strokeColor = new Color(1, 1, 1, 0.4f);
        painter.lineWidth = 3f;

        foreach (var node in mapState.nodes)
        {
            if (node.connectedToNodes == null) continue;

            var startBtn = idToButtonMap[node.id];

            if (startBtn.worldBound.width == 0) continue;

            Vector2 startPos = GetBtnCenter(startBtn);

            foreach (var targetId in node.connectedToNodes)
            {
                if (!idToButtonMap.TryGetValue(targetId, out var endBtn)) continue;

                painter.BeginPath();
                painter.MoveTo(startPos);
                painter.LineTo(GetBtnCenter(endBtn));
                painter.Stroke();
            }
        }
    }

    Vector2 GetBtnCenter(Button btn)
    {
        Vector2 worldCenter = btn.worldBound.center;
        return linesLayer.WorldToLocal(worldCenter);
    }

    #endregion

    #region Logic

    void UpdateButtonStates()
    {
        var available = mapState.GetAvailableNodes();

        foreach (var node in mapState.nodes)
        {
            var btn = idToButtonMap[node.id];
            bool isAvailable = available.Any(n => n.id == node.id);
            bool isVisited = node.id == mapState.currentNodeId || mapState.IsVisited(node);

            btn.SetEnabled(isAvailable);

            btn.RemoveFromClassList("node-available");
            btn.RemoveFromClassList("node-visited");

            if (isVisited) btn.AddToClassList("node-visited");
            else if (isAvailable) btn.AddToClassList("node-available");
        }
    }

    void OnNodeClicked(PathNodeData node)
    {
        mapState.MoveToNode(node);
        activeModifier.Set(mapState.GetModifier(node));
        SceneManager.LoadScene(node.row == mapState.TotalRows - 1 ? "BossScene" : "GameScene");
    }

    void RegisterNodeEvents(Button btn, PathNodeData node)
    {
        btn.clicked += () => OnNodeClicked(node);
        btn.RegisterCallback<PointerEnterEvent>(evt => tooltipController.Show(node, mapState, evt.position));
        btn.RegisterCallback<PointerLeaveEvent>(evt => tooltipController.Hide());
    }

    #endregion
}