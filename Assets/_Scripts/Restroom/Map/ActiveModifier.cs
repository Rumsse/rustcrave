using UnityEngine;

[CreateAssetMenu(fileName = "ActiveModifier", menuName = "Restroom/Map/Active Modifier")]
public class ActiveModifier : ScriptableObject
{
    public PathModifierData current;

    public float EnemySpawnMultiplier => current != null ? current.EnemySpawnMultiplier : 1f;
    public float ResourceSpawnMultiplier => current != null ? current.ResourceSpawnMultiplier : 1f;
    public float PulsiteSpawnMultiplier => current != null ? current.PulsiteSpawnMultiplier : 1f;
    public bool IsCameraUnstable => current != null && current.IsCameraUnstable;

    public void Set(PathModifierData modifier)
    {
        current = modifier;
        //Debug.Log($"<color=green>[ActiveModifier]</color> new modifier set: {(current != null ? current.DisplayName : "NULL")}");
    }

    public void Clear()
    {
        current = null;
        //Debug.Log("<color=red>[ActiveModifier]</color> modifier cleared.");
    }
}