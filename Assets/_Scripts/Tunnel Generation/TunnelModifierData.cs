using System.Collections.Generic;
using TunnelSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "TunnelModifier", menuName = "Tunnel System/Tunnel Modifier")]
public class TunnelModifierData : ScriptableObject
{
    [SerializeField] private string modifierName;
    [SerializeField][TextArea] private string description;

    [Header("Generation Modifiers")]
    [SerializeField] private float doubleTunnelChanceMultiplier = 1f;
    [SerializeField] private float lengthMultiplier = 1f;

    [Header("Resource Modifiers")]
    [SerializeField] private float resourceSpawnMultiplier = 1f;
    [SerializeField] private List<string> allowedResourceTypes = new List<string>();

    [Header("Enemy Modifiers")]
    [SerializeField] private float enemySpawnMultiplier = 1f;
    [SerializeField] private float enemyDifficultyMultiplier = 1f;

    public string ModifierName => modifierName;
    public string Description => description;
    public float DoubleTunnelChanceMultiplier => doubleTunnelChanceMultiplier;
    public float LengthMultiplier => lengthMultiplier;
    public float ResourceSpawnMultiplier => resourceSpawnMultiplier;
    public float EnemySpawnMultiplier => enemySpawnMultiplier;
    public float EnemyDifficultyMultiplier => enemyDifficultyMultiplier;

    public void ApplyToConfig(TunnelGeneratorConfig config)
    {
    }
}