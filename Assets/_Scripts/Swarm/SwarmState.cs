using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SwarmState", menuName = "Swarm/Swarm State")]
public class SwarmState : ScriptableObject
{
    public event Action OnSwarmChanged;

    [SerializeField] private List<UnitSO> startingSwarm = new();
    [SerializeField] private List<SwarmUnitsData> swarmUnits = new();
    [SerializeField] private int maxSwarmSize = 8;

    [SerializeField] private InventorySO globalInventory;
    public InventorySO GlobalInventory => globalInventory;



    public int MaxSwarmSize => maxSwarmSize;

    public IReadOnlyList<SwarmUnitsData> SwarmUnits => swarmUnits;
    public int AliveCount => swarmUnits.Count(m => m.isAlive);

    public event Action<SwarmUnitsData> OnUnitAdded;

    public void Initialize()
    {
        swarmUnits.Clear();

        foreach (var type in startingSwarm)
            swarmUnits.Add(new SwarmUnitsData(type));

        OnSwarmChanged?.Invoke();
    }

    public void AddUnitToSwarm(UnitSO type)
    {
        if (AliveCount >= maxSwarmSize)
            return;
        var newUnit = new SwarmUnitsData(type);
        swarmUnits.Add(newUnit);

        OnUnitAdded?.Invoke(newUnit);
        OnSwarmChanged?.Invoke();
    }

    public void MarkDead(string unitId)
    {
        var unit = swarmUnits.FirstOrDefault(m => m.id == unitId);

        if (unit == null)
            return;

        unit.isAlive = false;

        RemoveDead();
        OnSwarmChanged?.Invoke();
    }

    public void RemoveDead()
    {
        swarmUnits.RemoveAll(m => !m.isAlive);
        OnSwarmChanged?.Invoke();
    }

    public List<SwarmUnitsData> GetAliveUnits()
    {
        return swarmUnits.Where(u => u.isAlive).ToList();
    }

    public void Reset() => swarmUnits.Clear();
}