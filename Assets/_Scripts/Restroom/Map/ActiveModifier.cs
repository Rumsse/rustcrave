using UnityEngine;

[CreateAssetMenu(fileName = "ActiveModifier", menuName = "Restroom/Map/Active Modifier")]
public class ActiveModifier : ScriptableObject
{
    public PathModifierData current;

    public float EnemySpawnMultiplier => current != null ? current.EnemySpawnMultiplier : 1f;
    public float MimicSpawnMultiplier => current != null ? current.MimicSpawnMultiplier : 1f;
    public float ResourceSpawnMultiplier => current != null ? current.ResourceSpawnMultiplier : 1f;
    public float PulsiteSpawnMultiplier => current != null ? current.PulsiteSpawnMultiplier : 1f;
    public float SparkliteSpawnMultiplier => current != null ? current.SparkliteSpawnMultiplier : 1f;
    public bool IsCameraUnstable => current != null && current.IsCameraUnstable;
    public bool IsVoidChase => current != null && current.IsVoidChase;
    public bool IsFalseSilence => current != null && current.IsFalseSilence;
    public bool IsMoleTerritory => current != null && current.IsMoleTerritory;

    public void Set(PathModifierData modifier) => current = modifier;

    public void Clear() => current = null;
}