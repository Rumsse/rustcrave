using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class UnitSpawnOnTransformList : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private Transform _unitPrefab;
    [SerializeField] private float _spawnRadius = 2f;

    public void ZZ_SpawnUnitAtRandomPoint()
    {
        if (!isActiveAndEnabled || _spawnPoints.Count == 0)
            return;

        SpawnUnit(_spawnPoints[Random.Range(0, _spawnPoints.Count)].position);
    }

    public void ZZ_SpawnUnitAtIndex(int index)
    {
        if (!isActiveAndEnabled || index < 0 || index >= _spawnPoints.Count)
            return;

        SpawnUnit(_spawnPoints[index].position);
    }

    public void ZZ_SpawnUnitAtSpecificPoint(Transform spawnPoint)
    {
        if (!isActiveAndEnabled || !spawnPoint)
            return;

        SpawnUnit(spawnPoint.position);
    }

    private void SpawnUnit(Vector3 center)
    {
        var unitInstance = PoolManager.Instance.Get(_unitPrefab);
        if (unitInstance == null)
        {
            Debug.LogError("Failed to get unit instance from pool.");
            return;
        }

        Vector3 randomOffset = Random.insideUnitSphere * _spawnRadius;
        randomOffset.y = 0f;

        Vector3 targetPosition = center + randomOffset;
        Vector3 finalPosition = NavMeshUtils.GetNearestNavMeshPoint(targetPosition);

        unitInstance.position = finalPosition;

        var agent = unitInstance.GetComponentInChildren<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent missing on spawned unit.");
            return;
        }

        agent.Warp(finalPosition);
    }

    private void OnDrawGizmosSelected()
    {
        if (_spawnPoints == null)
            return;

        Gizmos.color = Color.green;
        foreach (var point in _spawnPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawWireSphere(point.position, _spawnRadius);
        }
    }
}