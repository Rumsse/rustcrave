using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Tunnel System/Enemy Config")]
public class EnemyConfig : BaseSpawnConfig
{
    [SerializeField] private int maxSpawnsPerSegment = 2;

    public override int MaxSpawnsPerSegment =>
        Mathf.RoundToInt(maxSpawnsPerSegment * (activeModifier != null ? activeModifier.EnemySpawnMultiplier : 1f));
}