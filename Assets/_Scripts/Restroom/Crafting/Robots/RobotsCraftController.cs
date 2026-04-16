using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RobotsCraftController : MonoBehaviour
{
    [Serializable]
    public struct RobotCraftData
    {
        public string ButtonId;
        public string ContainerId;
        public CraftingRecipe Recipe;
    }

    [SerializeField] private List<RobotCraftData> robotsCraftDataList;

    private Action<UnitSO> requestCraftRobot;
    private GlobalInventorySO globalInventory;
    private InfoTooltipController infoTooltip;



    public void Initialize(VisualElement root, Action<UnitSO> onCraftRequested, GlobalInventorySO inventory)
    {
        requestCraftRobot = onCraftRequested;
        globalInventory = inventory;

        var infoBtn = root.Q<Button>("btn-info-robots");
        var infoPanel = root.Q<VisualElement>("robots-info-panel");

        if (infoPanel != null)
            infoTooltip = new InfoTooltipController(infoPanel);

        if (infoBtn != null && infoTooltip != null)
        {
            infoBtn.RegisterCallback<PointerEnterEvent>(evt => infoTooltip.Show(evt.position));
            infoBtn.RegisterCallback<PointerLeaveEvent>(evt => infoTooltip.Hide());
            infoBtn.RegisterCallback<PointerMoveEvent>(evt => infoTooltip.UpdatePosition(evt.position));
        }

        foreach (var data in robotsCraftDataList)
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
        if(infoPanel == null) return;

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
        requestCraftRobot?.Invoke(recipe.CraftedUnit);
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