using UnityEngine;

[CreateAssetMenu(fileName = "PathModifier", menuName = "Restroom/Map/Path Modifier")]
public class PathModifierData : ScriptableObject
{
    [SerializeField] PathModifier type;
    [SerializeField] float enemySpawnMultiplier = 1f;
    [SerializeField] float mimicSpawnMultiplier = 1f;
    [SerializeField] float resourceSpawnMultiplier = 1f;
    [SerializeField] float pulsiteSpawnMultiplier = 1f;
    [SerializeField] float sparkliteSpawnMultiplier = 1f;
    [SerializeField] bool isCameraUnstable;
    [SerializeField] bool isVoidChase;
    [SerializeField] bool isFalseSilence;
    [SerializeField] bool isMoleTerritory;
    [SerializeField] bool isFragileCrust;

    [SerializeField] string displayName;
    [SerializeField] string description;
    [SerializeField] Sprite icon;

    public PathModifier Type => type;
    public float EnemySpawnMultiplier => enemySpawnMultiplier;
    public float MimicSpawnMultiplier => mimicSpawnMultiplier;
    public float ResourceSpawnMultiplier => resourceSpawnMultiplier;
    public float PulsiteSpawnMultiplier => pulsiteSpawnMultiplier;
    public float SparkliteSpawnMultiplier => sparkliteSpawnMultiplier;
    public bool IsCameraUnstable => isCameraUnstable;
    public bool IsVoidChase => isVoidChase;
    public bool IsFalseSilence => isFalseSilence;
    public bool IsMoleTerritory => isMoleTerritory;
    public bool IsFragileCrust => isFragileCrust;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
}