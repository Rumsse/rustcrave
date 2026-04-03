using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SwarmUnitsData
{
    public event Action<int> OnHealthRestored;
    public event Action<float> OnEnergyRestored;

    public string id;
    public UnitSO unitType;
    public int currentHP;
    public float currentEnergy;
    public bool isAlive;
    public List<GadgetSO> assignedGadgets = new List<GadgetSO>();

    public SwarmUnitsData(UnitSO type)
    {
        id = Guid.NewGuid().ToString();
        unitType = type;
        currentHP = type.maxHP;
        currentEnergy = type.maxEnergy;
        isAlive = true;

        var defaultEquipment = type.Prefab.GetComponentInChildren<UnitEquipment>();
        if (defaultEquipment != null)
            assignedGadgets.AddRange(defaultEquipment.GetEquippedGadgets());
    }

    public void RestoreEnergy(float amount)
    {
        if (!isAlive || unitType == null) return;

        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, GetTotalMaxEnergy());

        OnEnergyRestored?.Invoke(currentEnergy);
    }

    public void RestoreHealth(int amount)
    {
        if (!isAlive || unitType == null) return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, GetTotalMaxHP());

        OnHealthRestored?.Invoke(currentHP);
    }

    public int GetTotalMaxHP() => Mathf.RoundToInt(GetTotalStat(unitType.maxHP, StatsType.MaxHP));
    public int GetTotalMaxEnergy() => Mathf.RoundToInt(GetTotalStat(unitType.maxEnergy, StatsType.MaxEnergy));
    public int GetTotalDamage() => Mathf.RoundToInt(GetTotalStat(unitType.damage, StatsType.Damage));
    public float GetTotalMiningPower() => GetTotalStat(unitType.miningPower, StatsType.MiningPower);
    public float GetTotalSpeed() => GetTotalStat(unitType.moveSpeed, StatsType.Speed);
    public int GetTotalCapacity() => Mathf.RoundToInt(GetTotalStat(unitType.carryCapacity, StatsType.CarryCapacity));

    private float GetTotalStat(float baseValue, StatsType statType)
    {
        float total = baseValue;

        if (assignedGadgets == null)
            return total;

        foreach (var gadget in assignedGadgets)
        {
            if (gadget != null && gadget.modifiedStat == statType)
                total += gadget.statIncreaseAmount;
        }

        return total;
    }
}