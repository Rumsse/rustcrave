using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSpawnConfig : ScriptableObject, ISpawnConfig
{
    [SerializeField] protected List<SpawnEntry> entries = new List<SpawnEntry>();
    [SerializeField] protected ActiveModifier activeModifier;

    public IReadOnlyList<SpawnEntry> Entries => entries;
    public abstract int MaxSpawnsPerSegment { get; }

    public float TotalChance
    {
        get
        {
            float total = 0f;
            foreach (var entry in entries)
                total += GetModifiedChance(entry);

            return total;
        }
    }

    public GameObject GetRandomPrefab()
    {
        if (entries.Count == 0)
            return null;

        float total = TotalChance;
        if (total <= 0f)
            return null;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var entry in entries)
        {
            cumulative += GetModifiedChance(entry);
            if (roll <= cumulative)
                return entry.Prefab;
        }

        return entries[^1].Prefab;
    }

    public virtual float GetModifiedChance(SpawnEntry entry) => entry.SpawnChance;

    public bool Validate() => SpawnHelper.ValidateEntries(entries, name);
}