using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Tunnel System/Enemy Config")]
public class EnemyConfig : BaseSpawnConfig
{
    [SerializeField] private int maxSpawnsPerSegment = 2;
    [SerializeField] private float falseSilenceMimicChance = 0.5f;
    [SerializeField] private float moleTerritoryChance = 0.5f;

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

        if (activeModifier.IsFalseSilence)
        {
            if (entry.SpawnTag == SpawnTag.Standard)
                return 0f;

            if (entry.SpawnTag == SpawnTag.Mimic)
                return falseSilenceMimicChance;
        }

        if (activeModifier.IsMoleTerritory)
        {
            if (entry.SpawnTag == SpawnTag.Standard)
                return 0f;

            if (entry.SpawnTag == SpawnTag.Mole)
                return moleTerritoryChance;
        }

        return entry.SpawnChance * activeModifier.EnemySpawnMultiplier;
    }
}