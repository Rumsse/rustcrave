using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Tunnel System/Enemy Config")]
public class EnemyConfig : ScriptableObject, ISpawnConfig
{
    [SerializeField] private List<SpawnEntry> enemies = new List<SpawnEntry>();
    [SerializeField] private int maxSpawnsPerSegment = 2;

    public IReadOnlyList<SpawnEntry> Entries => enemies;
    public int MaxSpawnsPerSegment => maxSpawnsPerSegment;
    public float TotalChance => cachedTotalChance;

    private float cachedTotalChance;

    private void OnEnable() => RecalculateTotalChance();
    private void OnValidate() => RecalculateTotalChance();

    private void RecalculateTotalChance()
    {
        cachedTotalChance = 0f;
        foreach (var entry in enemies)
            cachedTotalChance += entry.SpawnChance;
    }

    public bool Validate() => SpawnHelper.ValidateConfig(this, name);
}