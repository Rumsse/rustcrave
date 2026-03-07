using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosePathController : MonoBehaviour
{
    [SerializeField] MapState mapState;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] VisualTreeAsset tooltipAsset;
    [SerializeField] string mainGameScene = "new Tunel Generation Rumsse";
    [SerializeField] string mainBossScene = "boss map";
    [SerializeField] bool debugMode = true;

    readonly Dictionary<Button, PathNodeData> buttonNodeMap = new();

    PathNodeTooltipController tooltipController;
    Label debugLabel;

    #region Initialization

    public void Initialize(VisualElement panelRoot, VisualElement tooltipLayer)
    {
        if (mapState.nodes.Count == 0)
            mapState.Initialize();

        if (tooltipController == null)
            tooltipController = new PathNodeTooltipController(tooltipLayer, tooltipAsset);

        buttonNodeMap.Clear();
        BindButtons(panelRoot);
        UpdateButtonStates();

        if (debugMode)
            SetupDebugLabel(panelRoot);
    }

    void BindButtons(VisualElement root)
    {
        foreach (var node in mapState.nodes)
        {
            var btn = root.Q<Button>($"node-{node.row}-{node.column}");

            if (btn == null)
            {
                Debug.LogWarning($"Button node-{node.row}-{node.column} not found!");
                continue;
            }

            buttonNodeMap[btn] = node;
            RegisterNodeEvents(btn, node);

            if (debugMode)
                SetupDebugButton(btn, node);
        }
    }

    void RegisterNodeEvents(Button btn, PathNodeData node)
    {
        btn.clicked += () => OnNodeClicked(node);

        if (node.row == mapState.TotalRows - 1)
            return;

        btn.RegisterCallback<PointerEnterEvent>(evt =>
            tooltipController.Show(node, mapState, evt.position));

        btn.RegisterCallback<PointerLeaveEvent>(evt =>
            tooltipController.Hide());

        btn.RegisterCallback<PointerMoveEvent>(evt =>
            tooltipController.UpdatePosition(evt.position));

        btn.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button == 1)
                OnNodeRightClicked(node);
        });
    }

    #endregion

    #region Button States

    void UpdateButtonStates()
    {
        var available = mapState.GetAvailableNodes();

        foreach (var (btn, node) in buttonNodeMap)
        {
            bool isAvailable = available.Contains(node);
            bool isVisited = node.row <= mapState.currentRow;

            btn.SetEnabled(isAvailable);

            btn.RemoveFromClassList("node-available");
            btn.RemoveFromClassList("node-visited");
            btn.RemoveFromClassList("node-locked");

            if (isVisited)
                btn.AddToClassList("node-visited");
            else if (isAvailable)
                btn.AddToClassList("node-available");
            else
                btn.AddToClassList("node-locked");
        }
    }

    #endregion

    #region Node Selection

    void OnNodeClicked(PathNodeData node)
    {
        mapState.MoveToNode(node);

        var modifier = mapState.GetModifier(node);
        activeModifier.Set(modifier);

        bool isBoss = node.row == mapState.TotalRows - 1;

        string scene = isBoss ? mainBossScene : mainGameScene;
        SceneManager.LoadScene(scene);
    }

    void OnNodeRightClicked(PathNodeData node)
    {
        if (mapState.IsNodeScanned(node))
            return;

        mapState.ScanNode(node);
        tooltipController.RefreshContent();
    }

    #endregion

    #region Debug

    void SetupDebugButton(Button btn, PathNodeData node)
    {
        var modifier = mapState.GetModifier(node);
        btn.text = $"{modifier.DisplayName}\n[{node.row},{node.column}]";
        btn.tooltip = $"Row: {node.row}\nCol: {node.column}\n{modifier.DisplayName}\n{modifier.Description}";
    }

    void SetupDebugLabel(VisualElement root)
    {
        debugLabel = new Label();
        debugLabel.style.position = Position.Absolute;
        debugLabel.style.left = 10;
        debugLabel.style.bottom = 10;
        debugLabel.style.color = Color.yellow;
        debugLabel.style.fontSize = 14;
        debugLabel.text = BuildDebugText();
        root.Add(debugLabel);
    }

    string BuildDebugText()
    {
        var text = $"Current: row {mapState.currentRow}, col {mapState.currentColumn}\n";
        text += "Available nodes:\n";

        foreach (var node in mapState.GetAvailableNodes())
        {
            var mod = mapState.GetModifier(node);
            text += $"  [{node.row},{node.column}] {mod.DisplayName} (enemy x{mod.EnemySpawnMultiplier}, resource x{mod.ResourceSpawnMultiplier})\n";
        }

        return text;
    }

    #endregion
}