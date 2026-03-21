using FMOD;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitInfoPanelController : MonoBehaviour
{
    [SerializeField] UnitMaintanceController maintenanceController;

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

    SwarmUnitsData currentUnit;
    IVisualElementScheduledItem updateTask;

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
    }

    #endregion

    #region Panel Control

    public void OpenPanel(SwarmUnitsData unitData)
    {
        if (unitData == null || unitData.unitType == null)
            return;

        currentUnit = unitData;

        if (unitImage != null && currentUnit.unitType.robotSprite != null)
            unitImage.style.backgroundImage = new StyleBackground(currentUnit.unitType.robotSprite);

        if (nameLabel != null)
            nameLabel.text = currentUnit.unitType.robotName;

        if (descriptionLabel != null)
            descriptionLabel.text = currentUnit.unitType.robotDescription;

        if (abilityDescriptionLabel != null)
            abilityDescriptionLabel.text = currentUnit.unitType.abilityDescription;

        UpdateStats();

        updateTask?.Pause();
        updateTask = rootElement.schedule.Execute(UpdateStats).Every(100);
    }

    public void ClosePanel()
    {
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