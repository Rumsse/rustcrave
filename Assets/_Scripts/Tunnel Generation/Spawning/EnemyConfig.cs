using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Tunnel System/Enemy Config")]
public class EnemyConfig : BaseSpawnConfig
{
    [SerializeField] private int maxSpawnsPerSegment = 2;

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

        return entry.SpawnChance * activeModifier.EnemySpawnMultiplier;
    }
}