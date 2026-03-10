using System.Collections.Generic;

public interface ISpawnConfig
{
    IReadOnlyList<SpawnEntry> Entries { get; }
    int MaxSpawnsPerSegment { get; }
    float TotalChance { get; }
}