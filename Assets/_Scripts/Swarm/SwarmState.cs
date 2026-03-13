using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SwarmState", menuName = "Swarm/Swarm State")]
public class SwarmState : ScriptableObject
{
    [SerializeField] private List<UnitSO> startingSwarm = new();
    [SerializeField] private List<SwarmUnitsData> swarmUnits = new();
    [SerializeField] private int maxSwarmSize = 8;

    public int MaxSwarmSize => maxSwarmSize;

    public IReadOnlyList<SwarmUnitsData> SwarmUnits => swarmUnits;
    public int AliveCount => swarmUnits.Count(m => m.isAlive);

    public event Action<SwarmUnitsData> OnUnitAdded;

    public void Initialize()
    {
        swarmUnits.Clear();
        foreach (var type in startingSwarm)
            swarmUnits.Add(new SwarmUnitsData(type));
    }

    public void AddUnitToSwarm(UnitSO type)
    {
        var newUnit = new SwarmUnitsData(type);
        swarmUnits.Add(newUnit);
        OnUnitAdded?.Invoke(newUnit);
    }

    public void MarkDead(string unitId)
    {
        var unit = swarmUnits.FirstOrDefault(m => m.id == unitId);
        if (unit == null)
            return;

        unit.isAlive = false;
    }

    public void RemoveDead() => swarmUnits.RemoveAll(m => !m.isAlive);

    public void Reset() => swarmUnits.Clear();
}