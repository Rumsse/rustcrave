using UnityEngine;

[CreateAssetMenu(fileName = "ResourceConfig", menuName = "Tunnel System/Resource Config")]
public class ResourceConfig : BaseSpawnConfig
{
    [SerializeField] private int maxSpawnsPerSegment = 3;

    public override int BaseMaxSpawnsPerSegment => maxSpawnsPerSegment;

    public override int MaxSpawnsPerSegment
    {
        get
        {
            float ratio = BaseTotalChance > 0f ? TotalChance / BaseTotalChance : 1f;
            return Mathf.RoundToInt(maxSpawnsPerSegment * ratio);
        }
    }

    public override float GetModifiedChance(SpawnEntry entry)
    {
        if (activeModifier == null)
            return entry.SpawnChance;

        float modifiedChance = entry.SpawnChance * activeModifier.ResourceSpawnMultiplier;

        if (entry.SpawnTag == SpawnTag.Pulsite)
            modifiedChance *= activeModifier.PulsiteSpawnMultiplier;

        if (entry.SpawnTag == SpawnTag.Sparklite)
            modifiedChance *= activeModifier.SparkliteSpawnMultiplier;

        return modifiedChance;
    }
}