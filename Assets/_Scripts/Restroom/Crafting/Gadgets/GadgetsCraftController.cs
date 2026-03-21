using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GadgetsCraftController : MonoBehaviour
{
    [Serializable]
    public struct GadgetCraftData
    {
        public string ButtonId;
        public string ContainerId;
        public CraftingRecipe Recipe;
    }

    [SerializeField] private List<GadgetCraftData> gadgetCraftDataList;

    private Action<GadgetSO> requestCraftGadget;
    private GlobalInventorySO globalInventory;



    public void Initialize(VisualElement root, Action<GadgetSO> onCraftRequested, GlobalInventorySO inventory)
    {
        requestCraftGadget = onCraftRequested;
        globalInventory = inventory;

        var infoBtn = root.Q<Button>("btn-info-gadgets");
        var infoPanel = root.Q<VisualElement>("gadgets-info-panel");

        if (infoBtn != null && infoPanel != null)
        {
            infoBtn.clicked += () => ShowInfo(infoPanel);
            infoBtn.clicked -= () => ShowInfo(infoPanel);
        }

        foreach (var data in gadgetCraftDataList)
        {
            var button = root.Q<Button>(data.ButtonId);
            var container = root.Q<VisualElement>(data.ContainerId);

            if (container != null && data.Recipe != null)
                container.dataSource = data.Recipe;

            if (button == null)
                continue;

            button.clicked -= () => OnCraftClicked(data.Recipe);
            button.clicked += () => OnCraftClicked(data.Recipe);
        }

    }

    #region UI Logic

    public void ShowInfo(VisualElement infoPanel)
    {
        if (infoPanel == null) return;

        bool isHidden = infoPanel.style.display == DisplayStyle.None;
        infoPanel.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
    }

    #endregion

    #region Crafting Logic

    private void OnCraftClicked(CraftingRecipe recipe)
    {
        if (recipe == null)
            return;

        if (!CanAfford(recipe))
            return;

        ConsumeResources(recipe);
        requestCraftGadget?.Invoke(recipe.CraftedGadget);
    }

    private bool CanAfford(CraftingRecipe recipe)
    {
        foreach (var cost in recipe.Costs)
            if (GetResourceAmount(cost.Ore) < cost.Amount)
                return false;

        return true;
    }

    private void ConsumeResources(CraftingRecipe recipe)
    {
        foreach (var cost in recipe.Costs)
            globalInventory.RemoveItem(cost.Ore, cost.Amount);
    }

    private int GetResourceAmount(OreSO ore)
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
