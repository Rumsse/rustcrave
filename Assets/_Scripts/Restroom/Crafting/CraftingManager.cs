using UnityEngine;

public interface ICraftingService
{
    bool TryCraft(CraftingRecipe recipe);
}

public class CraftingManager : MonoBehaviour, ICraftingService // chyba nieaktualny, stary kod lol
{
    [SerializeField] GlobalInventoryData globalInventory;

    public bool TryCraft(CraftingRecipe recipe)
    {
        if (recipe == null)
            return false;

        if (!CanAfford(recipe))
            return false;

        ConsumeResources(recipe);
        return true;
    }

    #region Resource Spending Logic

    bool CanAfford(CraftingRecipe recipe)
    {
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

    int GetResourceAmount(OreData ore)
    {
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