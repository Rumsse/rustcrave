using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MapGenerator
{
    public static List<PathNodeData> Generate(int minRows, int maxRows, int minCols, int maxCols, int minTotalNodes, int maxTotalNodes, int modifierCount)
    {
        List<PathNodeData> bestGraph = null;
        int closestDiff = int.MaxValue;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            var nodes = GenerateRawGraph(minRows, maxRows, minCols, maxCols, modifierCount);
            PruneDeadEnds(nodes);

            int count = nodes.Count;

            if (count >= minTotalNodes && count <= maxTotalNodes)
                return nodes;

            int diff = Mathf.Min(Mathf.Abs(count - minTotalNodes), Mathf.Abs(count - maxTotalNodes));
            if (diff < closestDiff)
            {
                closestDiff = diff;
                bestGraph = nodes;
            }
        }

        if (bestGraph != null)
            Debug.LogWarning("Could not fit node count range perfectly. Best effort generated.");

        return bestGraph;
    }

    static List<PathNodeData> GenerateRawGraph(int minRows, int maxRows, int minCols, int maxCols, int modifierCount)
    {
        int rows = Random.Range(minRows, maxRows + 1);
        int cols = Random.Range(minCols, maxCols + 1);
        var nodes = new List<PathNodeData>();
        var nodesByRow = new Dictionary<int, List<PathNodeData>>();

        for (int r = 0; r < rows; r++)
        {
            nodesByRow[r] = new List<PathNodeData>();
            int nodesInRow = (r == rows - 1) ? 1 : Random.Range(2, cols + 1);
            float centerColumnIndex = (cols - 1) / 2f;

            for (int i = 0; i < nodesInRow; i++)
            {
                float colPos;
                if (r == rows - 1)
                    colPos = centerColumnIndex;
                else
                {
                    float offsetFromCenter = (i - (nodesInRow - 1) / 2f);
                    colPos = centerColumnIndex + offsetFromCenter;
                }

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

                var verticalConns = nextRow
                    .OrderBy(n => Mathf.Abs(n.columnPosition - node.columnPosition))
                    .Take(connections)
                    .Select(n => n.id);

                node.connectedToNodes = verticalConns.ToArray();
            }
        }

        for (int r = 1; r < rows - 1; r++)
        {
            var rowNodes = nodesByRow[r].OrderBy(n => n.columnPosition).ToList();

            for (int i = 0; i < rowNodes.Count - 1; i++)
            {
                if (Random.value < 0.25f)
                {
                    var currentConns = rowNodes[i].connectedToNodes.ToList();
                    if (!currentConns.Contains(rowNodes[i + 1].id))
                    {
                        currentConns.Add(rowNodes[i + 1].id);
                        rowNodes[i].connectedToNodes = currentConns.ToArray();
                    }

                    var nextConns = rowNodes[i + 1].connectedToNodes.ToList();
                    if (!nextConns.Contains(rowNodes[i].id))
                    {
                        nextConns.Add(rowNodes[i].id);
                        rowNodes[i + 1].connectedToNodes = nextConns.ToArray();
                    }
                }
            }
        }

        return nodes;
    }

    static void PruneDeadEnds(List<PathNodeData> nodes)
    {
        if (nodes.Count == 0) return;
        int totalRows = nodes.Max(n => n.row) + 1;
        bool changed;

        do
        {
            changed = false;
            var validIds = new HashSet<string>(nodes.Select(n => n.id));

            foreach (var node in nodes)
            {
                var validConns = node.connectedToNodes.Where(id => validIds.Contains(id)).ToArray();
                if (validConns.Length != node.connectedToNodes.Length)
                {
                    node.connectedToNodes = validConns;
                    changed = true;
                }
            }

            var hasIncoming = new HashSet<string>(nodes.SelectMany(n => n.connectedToNodes));
            int removedIn = nodes.RemoveAll(n => n.row > 0 && n.row < totalRows - 1 && !hasIncoming.Contains(n.id));
            if (removedIn > 0) changed = true;

            int removedOut = nodes.RemoveAll(n => n.row < totalRows - 1 && n.connectedToNodes.Length == 0);
            if (removedOut > 0) changed = true;

        } while (changed);
    }
}