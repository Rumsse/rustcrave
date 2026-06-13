using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MapGenerator
{
    public static List<PathNodeData> Generate(int minRows, int maxRows, int minColumns, int maxColumns, int minTotalNodes, int maxTotalNodes, int modifierCount)
    {
        List<PathNodeData> bestGraph = null;
        int closestDifference = int.MaxValue;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            var nodes = GenerateRawGraph(minRows, maxRows, minColumns, maxColumns, modifierCount);
            PruneDeadEnds(nodes);

            int count = nodes.Count;

            if (count >= minTotalNodes && count <= maxTotalNodes)
                return nodes;

            int difference = Mathf.Min(Mathf.Abs(count - minTotalNodes), Mathf.Abs(count - maxTotalNodes));
            if (difference < closestDifference)
            {
                closestDifference = difference;
                bestGraph = nodes;
            }
        }

        if (bestGraph != null)
            Debug.LogWarning("Could not fit node count range perfectly. Best effort generated.");

        return bestGraph;
    }

    static List<PathNodeData> GenerateRawGraph(int minRows, int maxRows, int minColumns, int maxColumns, int modifierCount)
    {
        int rows = Random.Range(minRows, maxRows + 1);
        int columns = Random.Range(minColumns, maxColumns + 1);
        var nodes = new List<PathNodeData>();
        var nodesByRow = new Dictionary<int, List<PathNodeData>>();

        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            nodesByRow[rowIndex] = new List<PathNodeData>();
            int nodeCountInRow = (rowIndex == rows - 1) ? 1 : Random.Range(2, columns + 1);
            float centerColumnIndex = (columns - 1) / 2f;

            for (int i = 0; i < nodeCountInRow; i++)
            {
                float columnPosition;
                if (rowIndex == rows - 1)
                {
                    columnPosition = centerColumnIndex;
                }
                else
                {
                    float offsetFromCenter = (i - (nodeCountInRow - 1) / 2f);
                    columnPosition = centerColumnIndex + offsetFromCenter;
                }

                var node = new PathNodeData(rowIndex, columnPosition, Random.Range(0, modifierCount));
                nodes.Add(node);
                nodesByRow[rowIndex].Add(node);
            }
        }

        for (int rowIndex = 0; rowIndex < rows - 1; rowIndex++)
        {
            foreach (var node in nodesByRow[rowIndex])
            {
                var nextRow = nodesByRow[rowIndex + 1];
                int connections = Random.Range(1, Mathf.Min(3, nextRow.Count + 1));

                var verticalConnections = nextRow
                    .OrderBy(n => Mathf.Abs(n.ColumnPosition - node.ColumnPosition))
                    .Take(connections)
                    .Select(n => n.Id)
                    .ToList();

                node.OverwriteConnections(verticalConnections);
            }
        }



        // OPTIONAL: it is for adding some horizontal connections for variety

        /*for (int rowIndex = 1; rowIndex < rows - 1; rowIndex++)
        {
            var rowNodes = nodesByRow[rowIndex].OrderBy(n => n.ColumnPosition).ToList();

            for (int i = 0; i < rowNodes.Count - 1; i++)
            {
                if (Random.value < 0.25f)
                {
                    rowNodes[i].AddConnection(rowNodes[i + 1].Id);
                    rowNodes[i + 1].AddConnection(rowNodes[i].Id);
                }
            }
        }*/

        return nodes;
    }

    static void PruneDeadEnds(List<PathNodeData> nodes)
    {
        if (nodes.Count == 0)
            return;

        int totalRows = nodes.Max(n => n.Row) + 1;
        bool hasChanged;

        do
        {
            hasChanged = false;
            var validIds = new HashSet<string>(nodes.Select(n => n.Id));

            foreach (var node in nodes)
            {
                var validConnections = node.ConnectedNodeIds.Where(id => validIds.Contains(id)).ToList();
                if (validConnections.Count != node.ConnectedNodeIds.Count)
                {
                    node.OverwriteConnections(validConnections);
                    hasChanged = true;
                }
            }

            var hasIncoming = new HashSet<string>(nodes.SelectMany(n => n.ConnectedNodeIds));

            int removedIncoming = nodes.RemoveAll(n => n.Row > 0 && n.Row < totalRows - 1 && !hasIncoming.Contains(n.Id));
            if (removedIncoming > 0)
                hasChanged = true;

            int removedOutgoing = nodes.RemoveAll(n => n.Row < totalRows - 1 && n.ConnectedNodeIds.Count == 0);
            if (removedOutgoing > 0)
                hasChanged = true;

        } while (hasChanged);
    }
}