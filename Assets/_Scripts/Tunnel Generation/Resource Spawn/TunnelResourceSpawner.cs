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

            SpawnResourcesInSegment(segment.transform, spawnPoints);
        }
    }

    private void SpawnResourcesInSegment(Transform segmentRoot, ResourceSpawnPoint[] spawnPoints)
    {
        int spawnCount = Mathf.Min(resourceConfig.MaxSpawnsPerSegment, spawnPoints.Length);
        var selectedPoints = SelectRandomPoints(spawnPoints, spawnCount);

        foreach (var point in selectedPoints)
        {
            GameObject prefab = resourceConfig.GetRandomResource();
            if (prefab == null)
                continue;

            Instantiate(prefab, point.transform.position, point.transform.rotation, segmentRoot);
        }
    }

    private List<ResourceSpawnPoint> SelectRandomPoints(ResourceSpawnPoint[] points, int count)
    {
        var available = new List<ResourceSpawnPoint>(points);
        var selected = new List<ResourceSpawnPoint>(count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            selected.Add(available[index]);
            available.RemoveAt(index);
        }

        return selected;
    }
}