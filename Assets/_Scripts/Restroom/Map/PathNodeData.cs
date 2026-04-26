using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathNodeData
{
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public int Row { get; private set; }
    [field: SerializeField] public float ColumnPosition { get; private set; }
    [field: SerializeField] public int ModifierIndex { get; private set; }
    [field: SerializeField] public List<string> ConnectedNodeIds { get; private set; }

    public PathNodeData(int row, float columnPosition, int modifierIndex)
    {
        Id = System.Guid.NewGuid().ToString();
        Row = row;
        ColumnPosition = columnPosition;
        ModifierIndex = modifierIndex;
        ConnectedNodeIds = new List<string>();
    }

    public void AddConnection(string targetId)
    {
        if (!ConnectedNodeIds.Contains(targetId))
            ConnectedNodeIds.Add(targetId);
    }

    public void OverwriteConnections(List<string> validConnections) => ConnectedNodeIds = validConnections;
}