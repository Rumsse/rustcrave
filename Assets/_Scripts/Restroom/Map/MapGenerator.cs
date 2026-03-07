using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static List<PathNodeData> Generate(int rows, int columns, int modifierCount)
    {
        var nodes = new List<PathNodeData>();
        int lastRow = rows - 1;

        for (int row = 0; row < rows; row++)
        {
            if (row == lastRow)
            {
                int bossCol = columns / 2;
                nodes.Add(new PathNodeData(row, bossCol, 0, new int[0]));
                continue;
            }

            for (int col = 0; col < columns; col++)
            {
                int modifier = Random.Range(0, modifierCount);

                int[] connections = row == lastRow - 1
                    ? new[] { columns / 2 }
                    : new[] { col };

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