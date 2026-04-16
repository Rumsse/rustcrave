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

    [HideInInspector] public List<PathNodeData> nodes = new();
    [HideInInspector] public HashSet<string> scannedNodes = new();
    [HideInInspector] public HashSet<string> visitedNodes = new();
    [HideInInspector] public string currentNodeId = string.Empty;

    public int TotalRows => nodes.Count > 0 ? nodes.Max(n => n.row) + 1 : 0;

    public int currentRow
    {
        get
        {
            if (string.IsNullOrEmpty(currentNodeId))
                return -1;

            var node = nodes.FirstOrDefault(n => n.id == currentNodeId);
            return node?.row ?? -1;
        }
    }

    #region Initialization

    public void Initialize() => InitializeProcedural(minRows, maxRows, minCols, maxCols);

    public void InitializeProcedural(int minRows, int maxRows, int minCols, int maxCols)
    {
        nodes = MapGenerator.Generate(minRows, maxRows, minCols, maxCols, availableModifiers.Count);
        scannedNodes.Clear();
        visitedNodes.Clear();
        currentNodeId = string.Empty;
    }

    #endregion

    #region Node Operations

    public PathModifierData GetModifier(PathNodeData node) => availableModifiers[node.modifierIndex];

    public List<PathNodeData> GetAvailableNodes()
    {
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
        visitedNodes.Add(node.id);
    }

    #endregion

    #region State Checks

    public bool IsVisited(PathNodeData node) => visitedNodes.Contains(node.id);

    public bool IsNodeScanned(PathNodeData node) => scannedNodes.Contains(node.id);

    public void ScanNode(PathNodeData node) => scannedNodes.Add(node.id);

    #endregion
}