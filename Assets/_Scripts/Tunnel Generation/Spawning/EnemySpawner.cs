using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private TunnelGenerator generator;
    [SerializeField] private EnemyConfig enemyConfig;
    [SerializeField] private Transform enemiesParent;
    [SerializeField] private float navMeshSampleRadius = 5f;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    private void OnEnable()
    {
        if (generator != null)
            generator.OnNavMeshReady += SpawnEnemies;
    }

    private void OnDisable()
    {
        if (generator != null)
            generator.OnNavMeshReady -= SpawnEnemies;
    }

    private void SpawnEnemies()
    {
        if (enemyConfig == null || !enemyConfig.Validate())
            return;

        var segments = generator.GetSpawnedSegments();

        foreach (var segment in segments)
        {
            if (segment == null)
                continue;

            var spawnPoints = segment.GetComponentsInChildren<EnemySpawnPoint>(true);
            if (spawnPoints.Length == 0)
                continue;

            var selected = SpawnHelper.SelectRandomPoints(spawnPoints, enemyConfig.MaxSpawnsPerSegment);

            foreach (var point in selected)
            {
                GameObject prefab = enemyConfig.GetRandomPrefab();
                if (prefab == null)
                    continue;

                if (!NavMesh.SamplePosition(point.transform.position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
                    continue;

                Transform parent = enemiesParent != null ? enemiesParent : segment.transform;
                GameObject enemy = Instantiate(prefab, hit.position, point.transform.rotation, parent);
                spawnedEnemies.Add(enemy);
            }
        }
    }

    public void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
            if (enemy != null)
                Destroy(enemy);

        spawnedEnemies.Clear();
    }
}