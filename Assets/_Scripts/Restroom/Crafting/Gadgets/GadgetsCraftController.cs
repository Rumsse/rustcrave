using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FMODUnity;
using PrimeTween;

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
    [SerializeField] private EventReference gadgetSound;

    private Func<GadgetSO, bool> requestCraftGadget;
    private GlobalInventorySO globalInventory;
    private InfoTooltipController infoTooltip;

    public void Initialize(VisualElement root, Func<GadgetSO, bool> onCraftRequested, GlobalInventorySO inventory)
    {
        requestCraftGadget = onCraftRequested;
        globalInventory = inventory;

        var infoBtn = root.Q<Button>("btn-info-gadgets");
        var infoPanel = root.Q<VisualElement>("gadgets-info-panel");

        if (infoPanel != null)
            infoTooltip = new InfoTooltipController(infoPanel);

        if (infoBtn != null && infoTooltip != null)
        {
            infoBtn.RegisterCallback<PointerEnterEvent>(evt => infoTooltip.Show(evt.position));
            infoBtn.RegisterCallback<PointerLeaveEvent>(evt => infoTooltip.Hide());
            infoBtn.RegisterCallback<PointerMoveEvent>(evt => infoTooltip.UpdatePosition(evt.position));
        }

        foreach (var data in gadgetCraftDataList)
        {
            var button = root.Q<Button>(data.ButtonId);
            var container = root.Q<VisualElement>(data.ContainerId);

            if (container != null && data.Recipe != null)
                container.dataSource = data.Recipe;

            if (button == null)
                continue;

            button.clicked -= () => OnCraftClicked(data.Recipe, button);
            button.clicked += () => OnCraftClicked(data.Recipe, button);
        }
    }

    #region UI Logic

    public void ShowInfo(VisualElement infoPanel)
    {
        if (infoPanel == null)
            return;

        bool isHidden = infoPanel.style.display == DisplayStyle.None;
        infoPanel.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void AnimateButton(Button button)
    {
        Tween.StopAll(button);
        Tween.Custom(
            target: button,
            startValue: 0f,
            endValue: 360f,
            duration: 0.85f,
            onValueChange: (btn, val) => btn.style.rotate = new StyleRotate(new Rotate(new Angle(val, AngleUnit.Degree))),
            ease: Ease.InOutBack
        );
    }

    #endregion

    #region Crafting Logic

    private void OnCraftClicked(CraftingRecipe recipe, Button button)
    {
        if (recipe == null)
            return;

        if (!CanAfford(recipe))
            return;

        if (requestCraftGadget == null || !requestCraftGadget.Invoke(recipe.CraftedGadget))
            return;

        ConsumeResources(recipe);
        AudioManager.PlayOneShot(gadgetSound);
        AnimateButton(button);
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