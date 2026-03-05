using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceConfig", menuName = "Tunnel System/Resource Config")]
public class ResourceConfig : ScriptableObject, ISpawnConfig
{
    [SerializeField] private List<SpawnEntry> resources = new List<SpawnEntry>();
    [SerializeField] private int maxSpawnsPerSegment = 3;

    public IReadOnlyList<SpawnEntry> Entries => resources;
    public int MaxSpawnsPerSegment => maxSpawnsPerSegment;
    public float TotalChance => cachedTotalChance;

    private float cachedTotalChance;

    private void OnEnable() => RecalculateTotalChance();
    private void OnValidate() => RecalculateTotalChance();

    private void RecalculateTotalChance()
    {
        cachedTotalChance = 0f;
        foreach (var entry in resources)
            cachedTotalChance += entry.SpawnChance;
    }

    public bool Validate() => SpawnHelper.ValidateConfig(this, name);
}