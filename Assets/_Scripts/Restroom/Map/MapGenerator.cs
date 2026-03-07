using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static List<PathNodeData> Generate(int rows, int columns, int modifierCount)
    {
        var nodes = new List<PathNodeData>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int modifier = row == rows - 1
                    ? 0
                    : Random.Range(0, modifierCount);

                var connections = GenerateConnections(col, columns);
                nodes.Add(new PathNodeData(row, col, modifier, connections));
            }
        }

        return nodes;
    }

    static int[] GenerateConnections(int currentCol, int totalColumns)
    {
        var connections = new List<int> { currentCol };

        if (currentCol > 0 && Random.value > 0.5f)
            connections.Add(currentCol - 1);

        if (currentCol < totalColumns - 1 && Random.value > 0.5f)
            connections.Add(currentCol + 1);

        return connections.ToArray();
    }
}