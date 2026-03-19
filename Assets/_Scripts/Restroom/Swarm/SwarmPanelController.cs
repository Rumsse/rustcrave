using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SwarmPanelController : MonoBehaviour
{
    [SerializeField] SwarmState swarmState;
    [SerializeField] VisualTreeAsset unitSlotTemplate;
    [SerializeField] VisualTreeAsset unitInfoPanelAsset;
    [SerializeField] UnitInfoPanelController unitInfoPanelController;

    VisualElement rootElement;
    VisualElement slotsContainer;
    VisualElement unitInfoContainer;
    Button btnCloseMain;

    readonly List<VisualElement> activeSlots = new();
    readonly Dictionary<string, VisualElement> subPanelCache = new();

    string activeTabKey;

    #region Initialization

    public void Initialize(VisualElement root, VisualElement layer)
    {
        rootElement = root;
        slotsContainer = root.Q<VisualElement>("swarm-slots-container");
        unitInfoContainer = root.Q<VisualElement>("unit-info-container");
        btnCloseMain = root.Q<Button>("btn-close");

        if (swarmState == null || slotsContainer == null || unitSlotTemplate == null)
            return;

        swarmState.OnSwarmChanged -= RebuildSwarmUI;
        swarmState.OnSwarmChanged += RebuildSwarmUI;

        swarmState.OnUnitAdded -= AddUnitSlot;
        swarmState.OnUnitAdded += AddUnitSlot;

        RebuildSwarmUI();

        root.schedule.Execute(UpdateStats).Every(100);
    }

    void OnDisable()
    {
        if (swarmState == null)
            return;

        swarmState.OnSwarmChanged -= RebuildSwarmUI;
        swarmState.OnUnitAdded -= AddUnitSlot;
    }

    #endregion

    #region Tab Management

    void OpenTab(string tabKey, VisualTreeAsset asset, SwarmUnitsData unitData)
    {
        if (activeTabKey == tabKey)
        {
            if (unitInfoPanelController != null)
                unitInfoPanelController.OpenPanel(unitData);

            return;
        }

        if (unitInfoContainer == null || asset == null)
            return;

        unitInfoContainer.Clear();

        if (!subPanelCache.TryGetValue(tabKey, out var panel))
        {
            panel = asset.CloneTree();
            panel.style.flexGrow = 1;
            subPanelCache[tabKey] = panel;

            if (unitInfoPanelController != null)
                unitInfoPanelController.Initialize(panel);
        }

        var innerBtnClose = panel.Q<Button>("btn-close");

        if (innerBtnClose != null)
        {
            innerBtnClose.clicked -= CloseCurrentTab;
            innerBtnClose.clicked += CloseCurrentTab;
        }

        if (btnCloseMain != null)
            btnCloseMain.style.display = DisplayStyle.None;

        unitInfoContainer.Add(panel);
        activeTabKey = tabKey;

        if (unitInfoPanelController != null)
            unitInfoPanelController.OpenPanel(unitData);
    }

    void CloseCurrentTab()
    {
        if (unitInfoContainer != null)
            unitInfoContainer.Clear();

        if (btnCloseMain != null)
            btnCloseMain.style.display = DisplayStyle.Flex;

        if (unitInfoPanelController != null)
            unitInfoPanelController.ClosePanel();

        activeTabKey = null;
    }

    #endregion

    #region UI Management

    void RebuildSwarmUI()
    {
        slotsContainer.Clear();
        activeSlots.Clear();

        foreach (var unit in swarmState.SwarmUnits)
        {
            if (!unit.isAlive)
                continue;

            AddUnitSlot(unit);
        }
    }

    void AddUnitSlot(SwarmUnitsData unitData)
    {
        if (!unitData.isAlive)
            return;

        var slot = unitSlotTemplate.CloneTree();
        slot.userData = unitData;

        var unitImage = slot.Q<VisualElement>("unit-image");

        if (unitData.unitType != null && unitData.unitType.robotSprite != null)
            unitImage.style.backgroundImage = new StyleBackground(unitData.unitType.robotSprite);

        slot.RegisterCallback<ClickEvent>(evt => OpenTab("unit-info-panel", unitInfoPanelAsset, unitData));

        slotsContainer.Add(slot);
        activeSlots.Add(slot);

        UpdateSingleSlotStats(slot, unitData);
    }

    #endregion

    #region Stats Update

    void UpdateStats()
    {
        foreach (var slot in activeSlots)
        {
            if (slot.userData is SwarmUnitsData unitData)
                UpdateSingleSlotStats(slot, unitData);
        }
    }

    void UpdateSingleSlotStats(VisualElement slot, SwarmUnitsData unitData)
    {
        var hpLabel = slot.Q<Label>("hp-label");
        var enLabel = slot.Q<Label>("en-label");

        if (hpLabel != null && unitData.unitType != null)
            hpLabel.text = $"HP {unitData.currentHP}/{unitData.unitType.maxHP}";

        if (enLabel != null && unitData.unitType != null)
        {
            float energyPercent = (unitData.currentEnergy / unitData.unitType.maxEnergy) * 100f;
            enLabel.text = $"EN {Mathf.RoundToInt(energyPercent)}%";
        }
    }

    #endregion
}