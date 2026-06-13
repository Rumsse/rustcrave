using System.Collections.Generic;
using UnityEngine;

public static class SpawnHelper
{
    public static List<T> SelectRandomPoints<T>(T[] points, int count)
    {
        var available = new List<T>(points);
        var selected = new List<T>(count);

        int selectCount = Mathf.Min(count, available.Count);

        for (int i = 0; i < selectCount; i++)
        {
            int index = Random.Range(0, available.Count);
            selected.Add(available[index]);

            available[index] = available[^1];
            available.RemoveAt(available.Count - 1);
        }

        return selected;
    }

    public static bool ValidateEntries(IReadOnlyList<SpawnEntry> entries, string configName)
    {
        if (entries.Count == 0)
        {
            Debug.LogWarning($"{configName} has no entries!");
            return false;
        }

        bool isValid = true;
        foreach (var entry in entries)
        {
            if (entry.Prefab == null)
            {
                Debug.LogError($"{configName} contains null prefab!");
                isValid = false;
            }

            if (entry.SpawnChance <= 0f)
                Debug.LogWarning($"{configName}: {entry.Prefab?.name} has zero or negative spawn chance.");
        }

        return isValid;
    }
}