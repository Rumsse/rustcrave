using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceConfig", menuName = "Tunnel System/Resource Config")]
public class ResourceConfig : ScriptableObject
{
    [SerializeField] private List<ResourceEntry> resources = new List<ResourceEntry>();
    [SerializeField] private int maxSpawnsPerSegment = 3;

    public int MaxSpawnsPerSegment => maxSpawnsPerSegment;

    private float totalChance;

    private void OnEnable() => RecalculateTotalChance();

    private void OnValidate() => RecalculateTotalChance();

    private void RecalculateTotalChance()
    {
        totalChance = 0f;
        foreach (var entry in resources)
            totalChance += entry.SpawnChance;
    }

    public GameObject GetRandomResource()
    {
        if (resources.Count == 0)
            return null;

        if (totalChance <= 0f)
            return null;

        float roll = UnityEngine.Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (var entry in resources)
        {
            cumulative += entry.SpawnChance;
            if (roll <= cumulative)
                return entry.Prefab;
        }

        return resources[resources.Count - 1].Prefab;
    }

    public bool Validate()
    {
        if (resources.Count == 0)
        {
            Debug.LogWarning("ResourceConfig has no resources!");
            return false;
        }

        bool isValid = true;
        foreach (var entry in resources)
        {
            if (entry.Prefab == null)
            {
                Debug.LogError("ResourceConfig contains null prefab!");
                isValid = false;
            }

            if (entry.SpawnChance <= 0f)
                Debug.LogWarning($"Resource {entry.Prefab?.name} has zero or negative spawn chance.");
        }

        return isValid;
    }

    [Serializable]
    public class ResourceEntry
    {
        [SerializeField] private GameObject prefab;
        [SerializeField][Range(0f, 1f)] private float spawnChance = 0.5f;

        public GameObject Prefab => prefab;
        public float SpawnChance => spawnChance;
    }
}