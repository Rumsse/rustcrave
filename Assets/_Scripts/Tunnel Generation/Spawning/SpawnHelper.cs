using System.Collections.Generic;
using UnityEngine;

public static class SpawnHelper
{
    public static GameObject GetRandomPrefab(ISpawnConfig config)
    {
        if (config.Entries.Count == 0 || config.TotalChance <= 0f)
            return null;

        float roll = Random.Range(0f, config.TotalChance);
        float cumulative = 0f;

        foreach (var entry in config.Entries)
        {
            cumulative += entry.SpawnChance;
            if (roll <= cumulative)
                return entry.Prefab;
        }

        return config.Entries[config.Entries.Count - 1].Prefab;
    }

    public static List<T> SelectRandomPoints<T>(T[] points, int count)
    {
        var available = new List<T>(points);
        var selected = new List<T>(count);

        int selectCount = Mathf.Min(count, available.Count);

        for (int i = 0; i < selectCount; i++)
        {
            int index = Random.Range(0, available.Count);
            selected.Add(available[index]);
            available.RemoveAt(index);
        }

        return selected;
    }

    public static bool ValidateConfig(ISpawnConfig config, string configName)
    {
        if (config.Entries.Count == 0)
        {
            Debug.LogWarning($"{configName} has no entries!");
            return false;
        }

        bool isValid = true;
        foreach (var entry in config.Entries)
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