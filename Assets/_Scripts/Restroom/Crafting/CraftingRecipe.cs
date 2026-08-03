using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Restroom/Crafting/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Unit")]
    public UnitData CraftedUnit;

    [Header("Restoration")]
    public int healthRestoreAmount;
    public int energyRestorePercentage;

    [Header("Gadgets")]
    public GadgetSO CraftedGadget;

    [Header("Costs")]
    public List<ResourceCost> Costs;


    [CreateProperty] public int SparkliteCost => GetResourceCost("sparklite");
    [CreateProperty] public int ScraponiteCost => GetResourceCost("scraponite");
    [CreateProperty] public int PulsiteCost => GetResourceCost("pulsite");

    int GetResourceCost(string id)
    {
        if (Costs == null)
            return 0;

        foreach (var cost in Costs)
        {
            if (cost.Ore != null && cost.Ore.oreName.ToLower() == id)
                return cost.Amount;
        }

        return 0;
    }
}

[Serializable]
public struct ResourceCost
{
    public OreSO Ore;
    public int Amount;
}