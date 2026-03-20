using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class UnitSpawnOnTransformList : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private Transform _unit;

    public void ZZ_SpawnUnitAtRandomPoint()
    {
        var unitInstance = PoolManager.Instance.Get(_unit);
        var pos = NavMeshUtils.GetNearestNavMeshPoint(_spawnPoints[Random.Range(0, _spawnPoints.Count)].position);
        unitInstance.transform.position = pos;
        unitInstance.GetComponentInChildren<NavMeshAgent>().Warp(pos);
    }
    
    public void ZZ_SpawnUnitAtIndex(int index)
    {
        var unitInstance = PoolManager.Instance.Get(_unit);
        var  pos = NavMeshUtils.GetNearestNavMeshPoint(_spawnPoints[index].position);
        unitInstance.transform.position = pos;
        unitInstance.GetComponentInChildren<NavMeshAgent>().Warp(pos);
    }

    // todo: Finish this when player is done
    // public void ZZ_SpawnUnitClosestToPlayer()
    // {
        // var unitInstance = PoolManager.Instance.Get(_unit);
    // }
}
