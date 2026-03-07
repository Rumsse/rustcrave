using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapState", menuName = "Restroom/Map State")]
public class MapState : ScriptableObject
{
    [SerializeField] List<PathModifierData> availableModifiers = new();
    [SerializeField] int totalRows = 4;
    [SerializeField] int totalColumns = 3;

    [HideInInspector] public int currentRow = -1;
    [HideInInspector] public int currentColumn = 1;
    [HideInInspector] public List<PathNodeData> nodes = new();

    public int TotalRows => totalRows;
    public int TotalColumns => totalColumns;

    public void Initialize()
    {
        currentRow = -1;
        currentColumn = 1;
        nodes = MapGenerator.Generate(totalRows, totalColumns, availableModifiers.Count);
    }

    public PathModifierData GetModifier(PathNodeData node) =>
        availableModifiers[node.modifierIndex];

    public List<PathNodeData> GetNodesAtRow(int row) =>
        nodes.FindAll(n => n.row == row);

    public List<PathNodeData> GetAvailableNodes()
    {
        int nextRow = currentRow + 1;
        var nextNodes = GetNodesAtRow(nextRow);

        if (currentRow < 0)
            return nextNodes;

        var currentNode = nodes.Find(n => n.row == currentRow && n.column == currentColumn);
        if (currentNode == null)
            return new List<PathNodeData>();

        return nextNodes.FindAll(n =>
            System.Array.Exists(currentNode.connectedToColumns, c => c == n.column));
    }

    public void MoveToNode(PathNodeData node)
    {
        currentRow = node.row;
        currentColumn = node.column;
    }
}