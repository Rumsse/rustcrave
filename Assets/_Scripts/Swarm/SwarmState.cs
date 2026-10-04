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

    [SerializeField] private List<UnitData> startingSwarm = new();
    [SerializeField] private List<SwarmUnitsData> swarmUnits = new();
    [SerializeField] private int maxSwarmSize = 8;
    [SerializeField] private GlobalInventoryData globalInventory;
    [SerializeField] private GadgetsGlobalInventory globalGadgetsInventory;

    public GlobalInventoryData GlobalInventory => globalInventory;
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

    public void AddUnitToSwarm(UnitData type)
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


    public SwarmSaveData GetSaveData()
    {
        var data = new SwarmSaveData();

        if (swarmUnits == null)
            return data;

        foreach (var unit in swarmUnits)
        {
            if (unit == null)
                continue;

            if (unit.unitType == null)
            {
                Debug.LogWarning("Skipping unit save: UnitType reference is missing.");
                continue;
            }

            var unitData = new UnitSaveData
            {
                id = unit.id,
                unitTypeName = unit.unitType.name,
                currentHP = unit.currentHP,
                currentEnergy = unit.currentEnergy,
                isAlive = unit.isAlive
            };

            if (unit.assignedGadgets != null)
            {
                foreach (var gadget in unit.assignedGadgets)
                {
                    if (gadget == null)
                        continue;

                    unitData.assignedGadgetNames.Add(gadget.name);
                }
            }

            data.units.Add(unitData);
        }

        return data;
    }

    public void LoadFromSave(SwarmSaveData data, GameDatabase db)
    {
        swarmUnits.Clear();

        foreach (var unitData in data.units)
        {
            var unitType = db.GetUnit(unitData.unitTypeName);

            if (unitType == null)
            {
                Debug.LogError($"Cannot load unit. {unitData.unitTypeName} is missing in GameDatabase!");
                continue;
            }

            var restoredUnit = new SwarmUnitsData(unitType);
            restoredUnit.LoadData(unitData, db);
            swarmUnits.Add(restoredUnit);
        }

        OnSwarmChanged?.Invoke();
    }
}