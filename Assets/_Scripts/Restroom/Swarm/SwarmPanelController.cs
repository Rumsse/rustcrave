using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SwarmPanelController : MonoBehaviour
{
    public event Action OnSwarmPanelOpened;
    public event Action OnSwarmPanelClosed;
    public event Action<SwarmUnitsData> OnUnitInfoOpened;
    public event Action OnUnitInfoClosed;

    [SerializeField] SwarmState swarmState;
    [SerializeField] VisualTreeAsset unitSlotTemplate;
    [SerializeField] VisualTreeAsset unitInfoPanelAsset;
    [SerializeField] UnitInfoPanelController unitInfoPanelController;
    [SerializeField] UnitMaintanceController maintenanceController;
    [SerializeField] StyleSheet quickManagementStyleSheet;

    VisualElement rootElement;
    VisualElement slotsContainer;
    VisualElement unitInfoContainer;
    Button btnCloseMain;
    Button btnQuickManagement;
    Label subtitleLabel;

    readonly List<VisualElement> activeSlots = new();
    readonly Dictionary<string, VisualElement> subPanelCache = new();

    string activeTabKey;
    bool isQuickManagementActive;

    const string DefaultSubtitle = "Click on the unit image to get more information about it";
    const string QuickSubtitle = "LMB to repair | RMB to charge";

    #region Initialization

    public void Initialize(VisualElement root, VisualElement layer)
    {
        rootElement = root;
        slotsContainer = root.Q<VisualElement>("swarm-slots-container");
        unitInfoContainer = root.Q<VisualElement>("unit-info-container");
        btnCloseMain = root.Q<Button>("btn-close");
        btnQuickManagement = root.Q<Button>("btn-quick-management");
        subtitleLabel = root.Q<Label>("subtitle-label");

        if (swarmState == null || slotsContainer == null || unitSlotTemplate == null)
            return;

        isQuickManagementActive = false;

        if (quickManagementStyleSheet != null)
            rootElement.styleSheets.Remove(quickManagementStyleSheet);

        if (subtitleLabel != null)
            subtitleLabel.text = DefaultSubtitle;

        if (btnQuickManagement != null)
        {
            btnQuickManagement.clicked -= ToggleQuickManagement;
            btnQuickManagement.clicked += ToggleQuickManagement;
        }

        if (btnCloseMain != null)
        {
            btnCloseMain.clicked -= HandleMainClose;
            btnCloseMain.clicked += HandleMainClose;
        }

        swarmState.OnSwarmChanged -= RebuildSwarmUI;
        swarmState.OnSwarmChanged += RebuildSwarmUI;

        swarmState.OnUnitAdded -= AddUnitSlot;
        swarmState.OnUnitAdded += AddUnitSlot;

        rootElement.RegisterCallback<DetachFromPanelEvent>(ResetPanel);

        RebuildSwarmUI();

        root.schedule.Execute(UpdateStats).Every(100);
    }

    public void NotifyPanelOpened() => OnSwarmPanelOpened?.Invoke();
    public void NotifyPanelClosed() => OnSwarmPanelClosed?.Invoke();

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

        OnUnitInfoOpened?.Invoke(unitData);
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

        OnUnitInfoClosed?.Invoke();
    }

    void ResetPanel(DetachFromPanelEvent evt)
    {
        CloseCurrentTab();

        if (!isQuickManagementActive)
            return;

        isQuickManagementActive = false;

        if (subtitleLabel != null)
            subtitleLabel.text = DefaultSubtitle;

        UpdateStyleSheet();
    }

    #endregion

    #region Quick Management

    void ToggleQuickManagement()
    {
        isQuickManagementActive = !isQuickManagementActive;

        if (subtitleLabel != null)
            subtitleLabel.text = isQuickManagementActive ? QuickSubtitle : DefaultSubtitle;

        UpdateStyleSheet();
    }

    void HandleMainClose()
    {
        if (!isQuickManagementActive)
            return;

        isQuickManagementActive = false;

        if (subtitleLabel != null)
            subtitleLabel.text = DefaultSubtitle;

        UpdateStyleSheet();
    }

    void UpdateStyleSheet()
    {
        if (rootElement == null || quickManagementStyleSheet == null)
            return;

        if (isQuickManagementActive)
        {
            rootElement.styleSheets.Add(quickManagementStyleSheet);

            foreach (var slot in activeSlots)
            {
                slot.styleSheets.Add(quickManagementStyleSheet);
                var img = slot.Q<VisualElement>("unit-image");
                img?.RemoveFromClassList("normal-manage-target");
                img?.AddToClassList("quick-manage-target");
            }
        }
        else
        {
            rootElement.styleSheets.Remove(quickManagementStyleSheet);

            foreach (var slot in activeSlots)
            {
                slot.styleSheets.Remove(quickManagementStyleSheet);
                var img = slot.Q<VisualElement>("unit-image");
                img?.RemoveFromClassList("quick-manage-target");
                img?.AddToClassList("normal-manage-target");
            }
        }
    }

    void HandleSlotInteraction(PointerUpEvent evt, SwarmUnitsData unitData)
    {
        if (isQuickManagementActive)
        {
            if (evt.button == 0)
                maintenanceController?.TryRepair(unitData);
            else if (evt.button == 1)
                maintenanceController?.TryCharge(unitData);

            return;
        }

        if (evt.button == 0)
            OpenTab("unit-info-panel", unitInfoPanelAsset, unitData);
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

        if (isQuickManagementActive && quickManagementStyleSheet != null)
        {
            slot.styleSheets.Add(quickManagementStyleSheet);
            unitImage?.AddToClassList("quick-manage-target");
        }
        else
        {
            unitImage?.AddToClassList("normal-manage-target");
        }

        if (unitData.unitType != null && unitData.unitType.robotSprite != null)
            unitImage.style.backgroundImage = new StyleBackground(unitData.unitType.robotSprite);

        if (unitImage != null)
            unitImage.RegisterCallback<PointerUpEvent>(evt => HandleSlotInteraction(evt, unitData));

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
        var enLabel = slot.Q<Label>("energy-label");

        if (hpLabel != null && unitData.unitType != null)
            hpLabel.text = $"HP {unitData.currentHP}/{unitData.GetTotalMaxHP()}";

        if (enLabel != null && unitData.unitType != null)
            enLabel.text = $"EN {Mathf.RoundToInt(unitData.currentEnergy)}%";
    }

    #endregion
}