using UnityEngine;

[CreateAssetMenu(fileName = "ActiveModifier", menuName = "Restroom/Map/Active Modifier")]
public class ActiveModifier : ScriptableObject
{
    public PathModifierData current;

    public float EnemySpawnMultiplier => current != null ? current.EnemySpawnMultiplier : 1f;
    public float ResourceSpawnMultiplier => current != null ? current.ResourceSpawnMultiplier : 1f;

    public void Set(PathModifierData modifier) => current = modifier;
    public void Clear() => current = null;
}