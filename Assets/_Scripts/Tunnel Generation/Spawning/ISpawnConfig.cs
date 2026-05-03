using System.Collections.Generic;
using UnityEngine;

public interface ISpawnConfig
{
    IReadOnlyList<SpawnEntry> Entries { get; }
    int MaxSpawnsPerSegment { get; }
    float TotalChance { get; }
    GameObject GetRandomPrefab();
    bool Validate();
}