using System;
using UnityEngine;

[Serializable]
public class SpawnEntry
{
    [SerializeField] private GameObject prefab;
    [SerializeField][Range(0f, 1f)] private float spawnChance = 0.5f;

    public GameObject Prefab => prefab;
    public float SpawnChance => spawnChance;
}