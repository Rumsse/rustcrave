using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SwarmState", menuName = "Swarm/Swarm State")]
public class SwarmState : ScriptableObject
{
    public event Action OnSwarmChanged;
    public event Action<SwarmUnitsData> OnUnitAdded;
    public event Action<SwarmUnitsData> OnUnitRemoved;

    [SerializeField] private List<UnitSO> startingSwarm = new();
    [SerializeField] private List<SwarmUnitsData> swarmUnits = new();
    [SerializeField] private int maxSwarmSize = 8;
    [SerializeField] private GlobalInventorySO globalInventory;
    [SerializeField] private GadgetsGlobalInventory globalGadgetsInventory;

    public GlobalInventorySO GlobalInventory => globalInventory;
    public GadgetsGlobalInventory GlobalGadgetsInventory => globalGadgetsInventory;

    public int MaxSwarmSize => maxSwarmSize;
    public IReadOnlyList<SwarmUnitsData> SwarmUnits => swarmUnits;
    public int AliveCount => swarmUnits.Count(m => m.isAlive);

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

        OnUnitRemoved?.Invoke(unit);

        RemoveDead();
        OnSwarmChanged?.Invoke();
    }

    public void RemoveDead()
    {
        swarmUnits.RemoveAll(m => !m.isAlive);
        OnSwarmChanged?.Invoke();
    }

    public List<SwarmUnitsData> GetAliveUnits() => swarmUnits.Where(u => u.isAlive).ToList();

    public void Reset() => Initialize();
}