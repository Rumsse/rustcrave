using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosePathController : MonoBehaviour
{
    [SerializeField] MapState mapState;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] string mainGameScene = "new Tunel Generation Rumsse";
    [SerializeField] bool debugMode = true;

    readonly Dictionary<Button, PathNodeData> buttonNodeMap = new();

    Label debugLabel;

    #region Initialization

    public void Initialize(VisualElement panelRoot)
    {
        if (mapState.nodes.Count == 0)
            mapState.Initialize();

        buttonNodeMap.Clear();
        BindButtons(panelRoot);
        UpdateButtonStates();

        if (debugMode)
            SetupDebugLabel(panelRoot);
    }

    void BindButtons(VisualElement root)
    {
        for (int row = 0; row < mapState.TotalRows; row++)
        {
            var nodesInRow = mapState.GetNodesAtRow(row);

            for (int col = 0; col < nodesInRow.Count; col++)
            {
                var node = nodesInRow[col];
                var btn = root.Q<Button>($"node-{row}-{col}");

                if (btn == null)
                {
                    Debug.LogWarning($"Button node-{row}-{col} not found!");
                    continue;
                }

                buttonNodeMap[btn] = node;
                btn.clicked += () => OnNodeClicked(node);

                if (debugMode)
                    SetupDebugButton(btn, node);
            }
        }
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

        Debug.Log($"Moving to row:{node.row} col:{node.column} modifier:{modifier.DisplayName}");
        SceneManager.LoadScene(mainGameScene);
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