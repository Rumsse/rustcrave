using UnityEngine;
using UnityEngine.AI;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private TunnelGenerator generator;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private int unitsToSpawn = 3;
    [SerializeField] private Transform unitsParent;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private float sampleRadius = 5f;
    [SerializeField] private float ringRadius = 2f;

    private void OnEnable()
    {
        if (generator != null) generator.OnNavMeshReady += SpawnUnits;
    }

    private void OnDisable()
    {
        if (generator != null) generator.OnNavMeshReady -= SpawnUnits;
    }

    private void SpawnUnits()
    {
        if (unitPrefab == null || unitsToSpawn <= 0) return;

        var origin = spawnOrigin != null ? spawnOrigin.position : transform.position;

        for (int i = 0; i < unitsToSpawn; i++)
        {
            var offset = Quaternion.Euler(0, 360f / unitsToSpawn * i, 0) * Vector3.forward * ringRadius;
            var pos = origin + offset;

            if (NavMesh.SamplePosition(pos, out var hit, sampleRadius, NavMesh.AllAreas))
                Instantiate(unitPrefab, hit.position, Quaternion.identity, unitsParent != null ? unitsParent : transform);
        }
    }
}