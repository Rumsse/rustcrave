using System;
using System.Collections.Generic;
using UnityEngine;

public class DisplayUnitSpawner : MonoBehaviour
{
    public event Action<Transform> OnNewUnitSpawned;

    [SerializeField] SwarmState swarmState;
    [SerializeField] Transform unitsParent;
    [SerializeField] List<Transform> spawnPoints = new();

    int currentSpawnIndex = 0;

    #region Unity Lifecycle

    void OnEnable()
    {
        if (swarmState != null)
            swarmState.OnUnitAdded += HandleNewUnitCrafted;
    }

    void OnDisable()
    {
        if (swarmState != null)
            swarmState.OnUnitAdded -= HandleNewUnitCrafted;
    }

    void Start() => SpawnDisplayModels();

    #endregion

    #region Spawning Logic

    void SpawnDisplayModels()
    {
        if (swarmState == null)
            return;

        currentSpawnIndex = 0;

        foreach (var swarmUnit in swarmState.SwarmUnits)
        {
            if (swarmUnit.isAlive)
                SpawnSingleUnit(swarmUnit);
        }
    }

    void HandleNewUnitCrafted(SwarmUnitsData newUnit)
    {
        Transform spawnedUnit = SpawnSingleUnit(newUnit);

        if (spawnedUnit != null)
            OnNewUnitSpawned?.Invoke(spawnedUnit);
    }

    Transform SpawnSingleUnit(SwarmUnitsData swarmUnit)
    {
        if (currentSpawnIndex >= spawnPoints.Count)
            return null;

        Transform spawnPoint = spawnPoints[currentSpawnIndex];

        GameObject inactiveHolder = new GameObject("InactiveHolder");
        inactiveHolder.SetActive(false);

        var go = Instantiate(swarmUnit.unitType.Prefab, spawnPoint.position, spawnPoint.rotation, inactiveHolder.transform);

        var allScripts = go.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var script in allScripts)
            DestroyImmediate(script);

        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null)
            DestroyImmediate(agent);

        var colliders = go.GetComponentsInChildren<Collider>(true);

        foreach (var col in colliders)
            DestroyImmediate(col);

        go.transform.SetParent(unitsParent != null ? unitsParent : spawnPoint);
        go.SetActive(true);

        Destroy(inactiveHolder);

        currentSpawnIndex++;

        return go.transform;
    }

    #endregion
}