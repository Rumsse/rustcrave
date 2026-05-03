using FMOD;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitInfoPanelController : MonoBehaviour
{
    public static event System.Action OnAnyGadgetEquipped;

    [SerializeField] UnitMaintanceController maintenanceController;
    [SerializeField] GadgetsGlobalInventory gadgetsGlobalInventory;
    [SerializeField] VisualTreeAsset gadgetIconTemplate;
    [SerializeField] SwarmState swarmState;

    VisualElement rootElement;
    VisualElement unitImage;
    Label nameLabel;
    Label descriptionLabel;
    Label hpLabel;
    Label energyLabel;
    Label abilityDescriptionLabel;
    Button btnRepair;
    Button btnCharge;

    Label hpStat;
    Label enStat;
    Label atkStat;
    Label digStat;
    Label spdStat;
    Label capStat;

    VisualElement gadgetPopup;
    VisualElement gadgetListContainer;
    Button btnClosePopup;
    List<Button> gadgetSlots = new();

    SwarmUnitsData currentUnit;
    IVisualElementScheduledItem updateTask;
    int currentEditingSlotIndex = -1;

    #region Initialization

    public void Initialize(VisualElement root)
    {
        rootElement = root;

        unitImage = root.Q<VisualElement>("unit-image");
        nameLabel = root.Q<Label>("unit-name");
        descriptionLabel = root.Q<Label>("unit-description");
        hpLabel = root.Q<Label>("hp-label");
        energyLabel = root.Q<Label>("energy-label");
        abilityDescriptionLabel = root.Q<Label>("ability-description");

        hpStat = root.Q<Label>("hp-stat");
        enStat = root.Q<Label>("en-stat");
        atkStat = root.Q<Label>("atk-stat");
        digStat = root.Q<Label>("dig-stat");
        spdStat = root.Q<Label>("spd-stat");
        capStat = root.Q<Label>("cap-stat");

        btnRepair = root.Q<Button>("btn-repair");
        btnCharge = root.Q<Button>("btn-charge");

        if (btnRepair != null)
        {
            btnRepair.clicked -= HandleRepairClick;
            btnRepair.clicked += HandleRepairClick;
        }

        if (btnCharge != null)
        {
            btnCharge.clicked -= HandleChargeClick;
            btnCharge.clicked += HandleChargeClick;
        }

        InitializeGadgetUI(root);
    }

    void InitializeGadgetUI(VisualElement root)
    {
        gadgetPopup = root.Q<VisualElement>("gadgets-popup");
        gadgetListContainer = root.Q<VisualElement>("gadgets-list-container");
        btnClosePopup = root.Q<Button>("btn-close-popup");

        if (btnClosePopup != null)
        {
            btnClosePopup.clicked -= CloseGadgetPopup;
            btnClosePopup.clicked += CloseGadgetPopup;
        }

        gadgetSlots.Clear();

        for (int i = 0; i < 2; i++)
        {
            var slot = root.Q<Button>($"gadget-slot-{i}");

            if (slot == null)
                continue;

            int index = i;
            slot.clicked -= () => OpenGadgetPopup(index);
            slot.clicked += () => OpenGadgetPopup(index);

            gadgetSlots.Add(slot);
        }

        CloseGadgetPopup();
    }

    #endregion

    #region Gadgets UI Logic

    void OpenGadgetPopup(int slotIndex)
    {
        if (gadgetPopup == null || gadgetsGlobalInventory == null || currentUnit == null)
            return;

        currentEditingSlotIndex = slotIndex;
        gadgetListContainer?.Clear();

        var unequipBtn = gadgetIconTemplate.Instantiate().Q<Button>();
        unequipBtn.text = "X";
        unequipBtn.style.backgroundImage = new StyleBackground();
        unequipBtn.clicked += () => EquipGadget(null);
        gadgetListContainer?.Add(unequipBtn);

        var availableGadgets = GetAvailableGadgets();

        foreach (var gadget in availableGadgets)
        {
            var iconBtn = gadgetIconTemplate.Instantiate().Q<Button>();

            if (gadget.gadgetIcon != null)
                iconBtn.style.backgroundImage = new StyleBackground(gadget.gadgetIcon);

            iconBtn.clicked += () => EquipGadget(gadget);
            gadgetListContainer?.Add(iconBtn);
        }

        gadgetPopup.style.display = DisplayStyle.Flex;
    }

    List<GadgetSO> GetAvailableGadgets()
    {
        var available = new List<GadgetSO>(gadgetsGlobalInventory.unlockedGadgets);

        if (swarmState == null)
            return available;

        foreach (var unit in swarmState.SwarmUnits)
        {
            if (unit.assignedGadgets == null)
                continue;

            foreach (var equipped in unit.assignedGadgets)
                if (equipped != null)
                    available.Remove(equipped);
        }

        return available;
    }

    void CloseGadgetPopup()
    {
        if (gadgetPopup == null)
            return;

        gadgetPopup.style.display = DisplayStyle.None;
        currentEditingSlotIndex = -1;
    }

    void EquipGadget(GadgetSO gadget)
    {
        if (currentUnit == null || currentEditingSlotIndex < 0)
            return;

        while (currentUnit.assignedGadgets.Count <= currentEditingSlotIndex)
            currentUnit.assignedGadgets.Add(null);

        currentUnit.assignedGadgets[currentEditingSlotIndex] = gadget;

        RefreshGadgetSlotsUI();
        CloseGadgetPopup();
        UpdateStats();

        if (gadget != null)
            OnAnyGadgetEquipped?.Invoke();
    }

    void RefreshGadgetSlotsUI()
    {
        if (currentUnit == null)
            return;

        for (int i = 0; i < gadgetSlots.Count; i++)
        {
            GadgetSO gadget = null;

            if (i < currentUnit.assignedGadgets.Count)
                gadget = currentUnit.assignedGadgets[i];

            if (gadget != null && gadget.gadgetIcon != null)
            {
                gadgetSlots[i].style.backgroundImage = new StyleBackground(gadget.gadgetIcon);
                gadgetSlots[i].text = string.Empty;
            }
            else
            {
                gadgetSlots[i].style.backgroundImage = new StyleBackground();
                gadgetSlots[i].text = string.Empty;
            }
        }
    }

    #endregion

    #region Panel Control

    public void OpenPanel(SwarmUnitsData unitData)
    {
        if (unitData == null || unitData.unitType == null)
            return;

        currentUnit = unitData;

        if (currentUnit.assignedGadgets == null)
            currentUnit.assignedGadgets = new List<GadgetSO>();

        if (unitImage != null && currentUnit.unitType.robotSprite != null)
            unitImage.style.backgroundImage = new StyleBackground(currentUnit.unitType.robotSprite);

        if (nameLabel != null)
            nameLabel.text = currentUnit.unitType.robotName;

        if (descriptionLabel != null)
            descriptionLabel.text = currentUnit.unitType.robotDescription;

        if (abilityDescriptionLabel != null)
            abilityDescriptionLabel.text = currentUnit.unitType.abilityDescription;

        UpdateStats();
        RefreshGadgetSlotsUI();

        updateTask?.Pause();
        updateTask = rootElement.schedule.Execute(UpdateStats).Every(100);
    }

    public void ClosePanel()
    {
        CloseGadgetPopup();
        updateTask?.Pause();
        currentUnit = null;
    }

    #endregion

    #region Button Actions

    void HandleRepairClick()
    {
        if (maintenanceController == null)
            return;

        if (maintenanceController.TryRepair(currentUnit))
            UpdateStats();
    }

    void HandleChargeClick()
    {
        if (maintenanceController == null)
            return;

        if (maintenanceController.TryCharge(currentUnit))
            UpdateStats();
    }

    #endregion

    #region Stats Update

    void UpdateStats()
    {
        if (currentUnit == null || !currentUnit.isAlive)
        {
            ClosePanel();
            return;
        }

        if (hpLabel != null)
            hpLabel.text = $"Health: {currentUnit.currentHP}/{currentUnit.GetTotalMaxHP()}";

        if (energyLabel != null)
            energyLabel.text = $"Energy: {Mathf.RoundToInt(currentUnit.currentEnergy)}%";

        if (hpStat != null)
            hpStat.text = $"Health: {currentUnit.GetTotalMaxHP()}";

        if (enStat != null)
            enStat.text = $"Energy: {currentUnit.GetTotalMaxEnergy()}";

        if (atkStat != null)
            atkStat.text = $"Attack: {currentUnit.GetTotalDamage()}";

        if (digStat != null)
            digStat.text = $"Dig: {currentUnit.GetTotalMiningPower()}";

        if (spdStat != null)
            spdStat.text = $"Speed: {currentUnit.GetTotalSpeed()}";

        if (capStat != null)
            capStat.text = $"Capacity: {currentUnit.GetTotalCapacity()}";
    }

    #endregion
}