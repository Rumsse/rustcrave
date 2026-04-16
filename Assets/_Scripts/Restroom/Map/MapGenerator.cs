using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MapGenerator
{
    public static List<PathNodeData> Generate(int minRows, int maxRows, int minCols, int maxCols, int modifierCount)
    {
        int rows = Random.Range(minRows, maxRows + 1);
        int cols = Random.Range(minCols, maxCols + 1);
        var nodes = new List<PathNodeData>();
        var nodesByRow = new Dictionary<int, List<PathNodeData>>();

        for (int r = 0; r < rows; r++)
        {
            nodesByRow[r] = new List<PathNodeData>();
            int nodesInRow = (r == rows - 1) ? 1 : Random.Range(2, cols + 1);

            for (int i = 0; i < nodesInRow; i++)
            {
                float colPos = (r == rows - 1) ? (cols - 1) / 2f : (float)i * (cols - 1) / (nodesInRow - 1);
                var node = new PathNodeData(r, colPos, Random.Range(0, modifierCount));
                nodes.Add(node);
                nodesByRow[r].Add(node);
            }
        }

        for (int r = 0; r < rows - 1; r++)
        {
            foreach (var node in nodesByRow[r])
            {
                var nextRow = nodesByRow[r + 1];
                int connections = Random.Range(1, Mathf.Min(3, nextRow.Count + 1));
                node.connectedToNodes = nextRow.OrderBy(n => Mathf.Abs(n.columnPosition - node.columnPosition))
                    .Take(connections)
                    .Select(n => n.id)
                    .ToArray();
            }
        }

        PruneDeadEnds(nodes, rows);
        return nodes;
    }

    static void PruneDeadEnds(List<PathNodeData> nodes, int totalRows)
    {
        bool changed;
        do
        {
            changed = false;
            var hasIncoming = new HashSet<string>(nodes.SelectMany(n => n.connectedToNodes ?? new string[0]));
            int removed = nodes.RemoveAll(n => n.row > 0 && n.row < totalRows - 1 && !hasIncoming.Contains(n.id));
            if (removed > 0) changed = true;

            int removedOut = nodes.RemoveAll(n => n.row < totalRows - 1 && (n.connectedToNodes == null || n.connectedToNodes.Length == 0));
            if (removedOut > 0) changed = true;
        } while (changed);
    }
}