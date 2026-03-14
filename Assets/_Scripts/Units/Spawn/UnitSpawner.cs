using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private TunnelGenerator generator;
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private Transform unitsParent;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private float sampleRadius = 5f;
    [SerializeField] private float ringRadius = 2f;

/*    private void Start()
    {
        swarmState.Initialize();
    }*/

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
        if (swarmState == null) return;

        var alive = swarmState.SwarmUnits;
        int count = swarmState.AliveCount;
        if (count <= 0)
            return;

        var origin = spawnOrigin != null ? spawnOrigin.position : transform.position;
        int spawnIndex = 0;

        foreach (var swarmUnit in alive)
        {
            if (!swarmUnit.isAlive)
                continue;

            var offset = Quaternion.Euler(0, 360f / count * spawnIndex, 0) * Vector3.forward * ringRadius;
            var pos = origin + offset;

            if (!NavMesh.SamplePosition(pos, out var hit, sampleRadius, NavMesh.AllAreas))
                continue;

            /*if (swarmUnit.unitType?.Prefab == null)
                continue;*/

            var go = Instantiate(swarmUnit.unitType.Prefab, hit.position, Quaternion.identity, unitsParent != null ? unitsParent : transform);
            var unit = go.GetComponent<Unit>();
            unit.Initialize(swarmUnit, swarmState);
            spawnIndex++;
        }
    }
}