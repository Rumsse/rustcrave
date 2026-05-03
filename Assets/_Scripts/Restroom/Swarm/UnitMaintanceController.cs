using UnityEngine;

public class UnitMaintanceController : MonoBehaviour
{
    public static event System.Action OnAnyUnitCharged;
    public static event System.Action OnAnyUnitHealthRestored;

    [SerializeField] GlobalInventorySO globalInventory;
    [SerializeField] CraftingRecipe repairCost;
    [SerializeField] CraftingRecipe chargeCost;

    #region Maintenance Logic

    public bool TryRepair(SwarmUnitsData unit)
    {
        if (unit == null || !unit.isAlive)
            return false;

        if (unit.currentHP >= unit.GetTotalMaxHP())
            return false;

        if (!CanAfford(repairCost))
            return false;

        ConsumeResources(repairCost);
        unit.RestoreHealth(repairCost.healthRestoreAmount);

        OnAnyUnitHealthRestored?.Invoke();

        return true;
    }

    public bool TryCharge(SwarmUnitsData unit)
    {
        if (unit == null || !unit.isAlive)
            return false;

        if (unit.currentEnergy >= unit.GetTotalMaxEnergy())
            return false;

        if (!CanAfford(chargeCost))
            return false;

        ConsumeResources(chargeCost);

        float chargeAmount = unit.unitType.maxEnergy * chargeCost.energyRestorePercentage / 100f;
        unit.RestoreEnergy(chargeAmount);

        OnAnyUnitCharged?.Invoke();

        return true;
    }

    #endregion

    #region Resource Handling

    bool CanAfford(CraftingRecipe recipe)
    {
        if (recipe == null)
            return false;

        foreach (var cost in recipe.Costs)
            if (GetResourceAmount(cost.Ore) < cost.Amount)
                return false;

        return true;
    }

    void ConsumeResources(CraftingRecipe recipe)
    {
        foreach (var cost in recipe.Costs)
            globalInventory.RemoveItem(cost.Ore, cost.Amount);
    }

    int GetResourceAmount(OreSO ore)
    {
        if (ore == null)
            return 0;

        string id = ore.oreName.ToLower();

        if (id == "sparklite")
            return globalInventory.Sparklite;

        if (id == "scraponite")
            return globalInventory.Scraponite;

        if (id == "pulsite")
            return globalInventory.Pulsite;

        return 0;
    }

    #endregion
}