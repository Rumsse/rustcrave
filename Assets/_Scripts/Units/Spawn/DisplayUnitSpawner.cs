using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class DisplayUnitSpawner : MonoBehaviour
{
    public event Action<Transform> OnNewUnitSpawned;
    public event Action<SwarmUnitsData> OnUnitClicked;

    [SerializeField] SwarmState swarmState;
    [SerializeField] Transform unitsParent;
    [SerializeField] List<Transform> spawnPoints = new();

    readonly Dictionary<SwarmUnitsData, Transform> spawnedModels = new();

    int currentSpawnIndex = 0;
    Camera mainCamera;

    #region Unity Lifecycle

    void Awake() => mainCamera = Camera.main;

    void OnEnable()
    {
        if (swarmState == null)
            return;

        swarmState.OnSwarmChanged += HandleSwarmChanged;
        swarmState.OnUnitAdded += HandleNewUnitCrafted;
        swarmState.OnUnitRemoved += HandleUnitDied;
    }

    void OnDisable()
    {
        if (swarmState == null)
            return;

        swarmState.OnSwarmChanged -= HandleSwarmChanged;
        swarmState.OnUnitAdded -= HandleNewUnitCrafted;
        swarmState.OnUnitRemoved -= HandleUnitDied;
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
            if (swarmUnit.isAlive)
                SpawnSingleUnit(swarmUnit);
    }

    void HandleSwarmChanged()
    {
        ClearAllModels();
        SpawnDisplayModels();
    }

    void ClearAllModels()
    {
        foreach (var modelTransform in spawnedModels.Values)
            if (modelTransform != null)
                Destroy(modelTransform.gameObject);

        spawnedModels.Clear();
        currentSpawnIndex = 0;
    }

    void HandleNewUnitCrafted(SwarmUnitsData newUnit)
    {
        Transform spawnedUnit = SpawnSingleUnit(newUnit);

        if (spawnedUnit != null)
            OnNewUnitSpawned?.Invoke(spawnedUnit);
    }

    void HandleUnitDied(SwarmUnitsData deadUnit)
    {
        if (!spawnedModels.TryGetValue(deadUnit, out Transform unitTransform))
            return;

        Destroy(unitTransform.gameObject);
        spawnedModels.Remove(deadUnit);
    }

    public Transform GetUnitTransform(SwarmUnitsData unitData)
    {
        if (unitData != null && spawnedModels.TryGetValue(unitData, out Transform t))
            return t;

        return null;
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

        var agent = go.GetComponent<NavMeshAgent>();
        if (agent != null)
            DestroyImmediate(agent);

        var rigidbodies = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rigidbodies)
            DestroyImmediate(rb);

        var lights = go.GetComponentsInChildren<Light>(true);
        foreach (var light in lights)
            DestroyImmediate(light);

        go.transform.SetParent(unitsParent != null ? unitsParent : spawnPoint);
        go.SetActive(true);

        Destroy(inactiveHolder);

        currentSpawnIndex++;
        spawnedModels[swarmUnit] = go.transform;

        return go.transform;
    }

    #endregion

    #region Interaction Logic

    public void CheckUnitClick()
    {
        if (mainCamera == null)
            return;

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit))
            return;

        foreach (var pair in spawnedModels)
        {
            if (hit.transform.IsChildOf(pair.Value))
            {
                OnUnitClicked?.Invoke(pair.Key);
                return;
            }
        }
    }

    #endregion

}