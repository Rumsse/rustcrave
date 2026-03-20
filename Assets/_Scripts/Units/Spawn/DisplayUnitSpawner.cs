using System.Collections.Generic;
using UnityEngine;

public class DisplayUnitSpawner : MonoBehaviour
{
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private Transform unitsParent;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private int currentSpawnIndex = 0;


    private void OnEnable()
    {
        if (swarmState != null)
            swarmState.OnUnitAdded += HandleNewUnitCrafted;
    }

    private void OnDisable()
    {
        if (swarmState != null)
            swarmState.OnUnitAdded -= HandleNewUnitCrafted;
    }

    private void Start() => SpawnDisplayModels();


    private void SpawnDisplayModels()
    {
        if (swarmState == null) return;

        currentSpawnIndex = 0;

        foreach (var swarmUnit in swarmState.SwarmUnits)
        {
            if (swarmUnit.isAlive)
                SpawnSingleUnit(swarmUnit);
        }
    }

    private void HandleNewUnitCrafted(SwarmUnitsData newUnit) => SpawnSingleUnit(newUnit);

    private void SpawnSingleUnit(SwarmUnitsData swarmUnit)
    {
        if (currentSpawnIndex >= spawnPoints.Count)
            return;

        Transform spawnPoint = spawnPoints[currentSpawnIndex];

        GameObject inactiveHolder = new GameObject("InactiveHolder");
        inactiveHolder.SetActive(false);

        var go = Instantiate(swarmUnit.unitType.Prefab, spawnPoint.position, spawnPoint.rotation, inactiveHolder.transform);

        var allScripts = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var script in allScripts)
        {
            DestroyImmediate(script); 
        }

        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) DestroyImmediate(agent);

        var colliders = go.GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
        {
            DestroyImmediate(col);
        }

        go.transform.SetParent(unitsParent != null ? unitsParent : spawnPoint);
        go.SetActive(true);

        Destroy(inactiveHolder); 

        currentSpawnIndex++;
    }
}