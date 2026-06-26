using System.Collections.Generic;
using UnityEngine;

public class TunnelResourceSpawner : MonoBehaviour
{
    [SerializeField] private ResourceConfig resourceConfig;

    public void SpawnResources(List<TunnelSegment> segments)
    {
        if (resourceConfig == null || !resourceConfig.Validate())
            return;

        foreach (var segment in segments)
        {
            if (segment == null)
                continue;

            var spawnPoints = segment.GetComponentsInChildren<ResourceSpawnPoint>(true);
            if (spawnPoints.Length == 0)
                continue;

            var selected = SpawnHelper.SelectRandomPoints(spawnPoints, resourceConfig.MaxSpawnsPerSegment);

            foreach (var point in selected)
            {
                GameObject prefab = resourceConfig.GetRandomPrefab();
                if (prefab == null)
                    continue;

                Quaternion randomRotation = point.transform.rotation * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                Instantiate(prefab, point.transform.position, randomRotation, segment.transform);
            }
        }
    }
}