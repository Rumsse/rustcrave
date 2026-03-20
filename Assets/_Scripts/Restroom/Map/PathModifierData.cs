using UnityEngine;

[CreateAssetMenu(fileName = "PathModifier", menuName = "Restroom/Map/Path Modifier")]
public class PathModifierData : ScriptableObject
{
    [SerializeField] PathModifier type;
    [SerializeField] float enemySpawnMultiplier = 1f;
    [SerializeField] float resourceSpawnMultiplier = 1f;
    [SerializeField] string displayName;
    [SerializeField] string description;

    public PathModifier Type => type;
    public float EnemySpawnMultiplier => enemySpawnMultiplier;
    public float ResourceSpawnMultiplier => resourceSpawnMultiplier;
    public string DisplayName => displayName;
    public string Description => description;
}