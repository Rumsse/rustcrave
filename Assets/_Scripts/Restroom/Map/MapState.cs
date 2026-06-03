using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MapState", menuName = "Restroom/Map/Map State")]
public class MapState : ScriptableObject
{
    [SerializeField] List<PathModifierData> availableModifiers = new();

    [SerializeField] int minRows = 4;
    [SerializeField] int maxRows = 5;
    [SerializeField] int minColumns = 4;
    [SerializeField] int maxColumns = 5;

    [SerializeField] int minTotalNodes = 11;
    [SerializeField] int maxTotalNodes = 14;

    [HideInInspector][SerializeField] List<PathNodeData> internalNodes = new();
    [HideInInspector][SerializeField] List<string> internalScannedNodeIds = new();
    [HideInInspector][SerializeField] List<string> internalVisitedNodeIds = new();

    public IReadOnlyList<PathNodeData> Nodes => internalNodes;
    [field: SerializeField, HideInInspector] public string CurrentNodeId { get; private set; } = string.Empty;

    public int TotalRows => internalNodes.Count > 0 ? internalNodes.Max(n => n.Row) + 1 : 0;
    public int TotalColumns => maxColumns;
    public int CurrentRow => GetCurrentRow();
    public int VisitedNodesCount => internalVisitedNodeIds.Count;

    public void Initialize()
    {
        internalNodes = MapGenerator.Generate(minRows, maxRows, minColumns, maxColumns, minTotalNodes, maxTotalNodes, availableModifiers.Count);
        ResetProgress();
    }

    public PathModifierData GetModifier(PathNodeData node) => availableModifiers[node.ModifierIndex];

    public IReadOnlyList<PathNodeData> GetAvailableNodes()
    {
        if (internalNodes.Count == 0)
            return new List<PathNodeData>();

        if (string.IsNullOrEmpty(CurrentNodeId))
            return internalNodes.Where(n => n.Row == 0).ToList();

        var currentNode = internalNodes.FirstOrDefault(n => n.Id == CurrentNodeId);

        if (currentNode == null || currentNode.ConnectedNodeIds == null)
            return new List<PathNodeData>();

        return internalNodes.Where(n => currentNode.ConnectedNodeIds.Contains(n.Id)).ToList();
    }

    public void MoveToNode(PathNodeData node)
    {
        CurrentNodeId = node.Id;

        if (!internalVisitedNodeIds.Contains(node.Id))
            internalVisitedNodeIds.Add(node.Id);

        Debug.Log($"[MapState] Moved to: {node.Id.Substring(0, 8)}...");
    }

    public bool IsVisited(PathNodeData node) => internalVisitedNodeIds.Contains(node.Id);

    public bool IsNodeScanned(PathNodeData node) => internalScannedNodeIds.Contains(node.Id);

    public void ScanNode(PathNodeData node)
    {
        if (!internalScannedNodeIds.Contains(node.Id))
            internalScannedNodeIds.Add(node.Id);
    }

    public void ResetProgress()
    {
        internalScannedNodeIds.Clear();
        internalVisitedNodeIds.Clear();
        CurrentNodeId = string.Empty;
    }

    int GetCurrentRow()
    {
        if (string.IsNullOrEmpty(CurrentNodeId))
            return -1;

        var node = internalNodes.FirstOrDefault(n => n.Id == CurrentNodeId);
        return node?.Row ?? -1;
    }

    public MapSaveData GetSaveData()
    {
        return new MapSaveData
        {
            nodes = new List<PathNodeData>(internalNodes),
            scannedNodeIds = new List<string>(internalScannedNodeIds),
            visitedNodeIds = new List<string>(internalVisitedNodeIds),
            currentNodeId = CurrentNodeId
        };
    }

    public void LoadFromSave(MapSaveData data)
    {
        internalNodes = data.nodes;
        internalScannedNodeIds = data.scannedNodeIds;
        internalVisitedNodeIds = data.visitedNodeIds;
        CurrentNodeId = data.currentNodeId;
    }
}