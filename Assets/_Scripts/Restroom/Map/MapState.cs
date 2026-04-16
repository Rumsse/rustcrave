using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MapState", menuName = "Restroom/Map/Map State")]
public class MapState : ScriptableObject
{
    [SerializeField] List<PathModifierData> availableModifiers = new();

    [SerializeField] int minRows = 4;
    [SerializeField] int maxRows = 6;
    [SerializeField] int minCols = 3;
    [SerializeField] int maxCols = 5;

    [SerializeField] int minTotalNodes = 11;
    [SerializeField] int maxTotalNodes = 14;

    [HideInInspector] public List<PathNodeData> nodes = new();
    [HideInInspector] public List<string> scannedNodes = new();
    [HideInInspector] public List<string> visitedNodes = new();
    [HideInInspector] public string currentNodeId = string.Empty;

    public int TotalRows => nodes.Count > 0 ? nodes.Max(n => n.row) + 1 : 0;
    public int TotalColumns => maxCols;

    public int currentRow => GetCurrentRow();

    int GetCurrentRow()
    {
        if (string.IsNullOrEmpty(currentNodeId))
            return -1;

        var node = nodes.FirstOrDefault(n => n.id == currentNodeId);
        return node?.row ?? -1;
    }

    public void Initialize()
    {
        nodes = MapGenerator.Generate(minRows, maxRows, minCols, maxCols, minTotalNodes, maxTotalNodes, availableModifiers.Count);
        scannedNodes.Clear();
        visitedNodes.Clear();
        currentNodeId = string.Empty;
    }

    public PathModifierData GetModifier(PathNodeData node) => availableModifiers[node.modifierIndex];

    public List<PathNodeData> GetAvailableNodes()
    {
        if (nodes.Count == 0)
            return new List<PathNodeData>();

        if (string.IsNullOrEmpty(currentNodeId))
            return nodes.Where(n => n.row == 0).ToList();

        var currentNode = nodes.FirstOrDefault(n => n.id == currentNodeId);

        if (currentNode == null || currentNode.connectedToNodes == null)
            return new List<PathNodeData>();

        return nodes.Where(n => currentNode.connectedToNodes.Contains(n.id)).ToList();
    }

    public void MoveToNode(PathNodeData node)
    {
        currentNodeId = node.id;

        if (!visitedNodes.Contains(node.id))
            visitedNodes.Add(node.id);

        Debug.Log($"[MapState] Moved to: {node.id.Substring(0, 8)}...");
    }

    public bool IsVisited(PathNodeData node) => visitedNodes.Contains(node.id);

    public bool IsNodeScanned(PathNodeData node) => scannedNodes.Contains(node.id);

    public void ScanNode(PathNodeData node)
    {
        if (!scannedNodes.Contains(node.id))
            scannedNodes.Add(node.id);
    }
}