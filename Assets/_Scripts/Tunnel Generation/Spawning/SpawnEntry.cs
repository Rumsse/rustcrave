using System;
using UnityEngine;

public enum SpawnTag
{
    Standard,
    Pulsite,
    Sparklite,
    Mimic
}

[Serializable]
public class SpawnEntry
{
    [SerializeField] private GameObject prefab;
    [SerializeField][Range(0f, 1f)] private float spawnChance = 0.5f;
    [SerializeField] private SpawnTag spawnTag = SpawnTag.Standard;

    public GameObject Prefab => prefab;
    public float SpawnChance => spawnChance;
    public SpawnTag SpawnTag => spawnTag;

}
