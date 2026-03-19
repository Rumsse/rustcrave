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
    Button btnRepair;
    Button btnCharge;

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
            hpLabel.text = $"Health: {currentUnit.currentHP}/{currentUnit.unitType.maxHP}";

        if (energyLabel != null)
        {
            float energyPercent = (currentUnit.currentEnergy / currentUnit.unitType.maxEnergy) * 100f;
            energyLabel.text = $"Energy: {Mathf.RoundToInt(energyPercent)}%";
        }
    }

    #endregion
}