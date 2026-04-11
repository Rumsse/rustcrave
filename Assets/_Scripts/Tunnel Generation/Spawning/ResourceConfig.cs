using UnityEngine;

[CreateAssetMenu(fileName = "ResourceConfig", menuName = "Tunnel System/Resource Config")]
public class ResourceConfig : BaseSpawnConfig
{
    [SerializeField] private int maxSpawnsPerSegment = 3;

    public override int MaxSpawnsPerSegment => maxSpawnsPerSegment;

    public override float GetModifiedChance(SpawnEntry entry)
    {
        if (entry.SpawnTag == SpawnTag.Pulsite && activeModifier != null)
            return entry.SpawnChance * activeModifier.ResourceSpawnMultiplier;

        return entry.SpawnChance;
    }
}