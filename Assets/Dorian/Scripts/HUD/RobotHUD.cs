using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RobotHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UnitStateUI stateUI;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private GameObject inventorySlotPrefab;

    private SwarmUnitsData unitData;
    private UnitInventoryUI inventoryUI;
    private UnitInventory currentInventory;

    private HealthManager healthManager;
    private EnergyManager energyManager;
    private MCFormController formController;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    public void OnClickIcon()
    {
        if (inventoryUI != null)
        {
            inventoryUI.TogglePanel();
        }
    }

    public void Setup(SwarmUnitsData data)
    {
        unitData = data;

        nameText.text = unitData.unitType.robotName;
        icon.sprite = unitData.unitType.robotSprite;

        UpdateHealthText(unitData.currentHP, (float)unitData.currentHP / unitData.unitType.maxHP);
        UpdateEnergyText(unitData.currentEnergy, (float)unitData.currentEnergy / unitData.unitType.maxEnergy);

        foreach (Transform child in inventoryPanel)
        {
            Destroy(child.gameObject);
        }
        uiSlots.Clear();

        for (int i = 0; i < unitData.unitType.carryCapacity; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, inventoryPanel);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            uiSlots.Add(slotUI);
            slotUI.ClearSlot();
        }

        TryAssignUnit();
        UnitRegistry.OnUnitRegistered += OnUnitRegistered;
    }

    private void TryAssignUnit()
    {
        Unit unitInstance = UnitRegistry.Get(unitData.id);
        if (unitInstance != null)
        {
            AssignUnitComponents(unitInstance);
        }
    }

    private void OnUnitRegistered(Unit unit)
    {
        if (unitData != null && unit.Id == unitData.id)
        {
            AssignUnitComponents(unit);
            UnitRegistry.OnUnitRegistered -= OnUnitRegistered;
        }
    }

    private void AssignUnitComponents(Unit unit)
    {
        if (stateUI != null) stateUI.SetUnit(unit);
        stateUI = unit.GetComponentInChildren<UnitStateUI>(true);
        if (stateUI != null)
        {
            stateUI.SetUnit(unit);
        }

        inventoryUI = unit.GetComponentInChildren<UnitInventoryUI>(true);
        if (inventoryUI != null)
        {
            inventoryUI.SetUnit(unit);
        }

        currentInventory = unit.GetComponent<UnitInventory>();
        if (currentInventory != null && currentInventory.InventorySO != null)
        {
            currentInventory.InventorySO.OnInventoryChanged += HandleInventoryChanged;
            RefreshInventoryVisuals();
        }

        healthManager = unit.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.onHealthPercentChange += HandleHealthChanged;
        }

        energyManager = unit.GetComponent<EnergyManager>();
        if (energyManager != null)
        {
            energyManager.onEnergyPercentChange += HandleEnergyChanged;
        }

        formController = unit.GetComponent<MCFormController>();
        if (formController != null)
        {
            formController.OnFormChanged += HandleFormChanged;
        }
    }

    private void HandleFormChanged(UnitSO newStats)
    {
        nameText.text = newStats.robotName;
        icon.sprite = newStats.robotSprite;
    }

    private void HandleHealthChanged(float percent)
    {
        if (healthManager == null || unitData == null) return;
        UpdateHealthText(healthManager.CurrentHP, percent);
    }

    private void HandleEnergyChanged(float percent)
    {
        if (energyManager == null || unitData == null) return;
        UpdateEnergyText(energyManager.CurrentEnergy, percent);
    }

    private void UpdateHealthText(int currentHp, float percent)
    {
        hpText.text = $"HP: {currentHp}/{unitData.unitType.maxHP}";
    }

    private void UpdateEnergyText(float currentEnergy, float percent)
    {
        int energyInt = Mathf.CeilToInt(currentEnergy);
        energyText.text = $"EN: {energyInt}/{unitData.unitType.maxEnergy}";
    }

    private void HandleInventoryChanged(object sender, EventArgs e)
    {
        RefreshInventoryVisuals();
    }

    private void RefreshInventoryVisuals()
    {
        if (currentInventory == null || currentInventory.InventorySO == null) return;

        int currentVisualSlotIndex = 0;

        foreach (var inventorySlot in currentInventory.InventorySO.inventoryItemList)
        {
            for (int i = 0; i < inventorySlot.amount; i++)
            {
                if (currentVisualSlotIndex < uiSlots.Count)
                {
                    uiSlots[currentVisualSlotIndex].SetItem(inventorySlot.item);
                    currentVisualSlotIndex++;
                }
            }
        }

        for (int i = currentVisualSlotIndex; i < uiSlots.Count; i++)
        {
            uiSlots[i].ClearSlot();
        }
    }

    private void OnDestroy()
    {
        UnitRegistry.OnUnitRegistered -= OnUnitRegistered;

        if (currentInventory != null && currentInventory.InventorySO != null)
        {
            currentInventory.InventorySO.OnInventoryChanged -= HandleInventoryChanged;
        }

        if (healthManager != null) healthManager.onHealthPercentChange -= HandleHealthChanged;
        if (energyManager != null) energyManager.onEnergyPercentChange -= HandleEnergyChanged;
        if (formController != null) formController.OnFormChanged -= HandleFormChanged;
    }
}