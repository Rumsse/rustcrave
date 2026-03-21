using System;
using System.Collections;
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

    [Header("Selection Visuals")]
    [SerializeField] private Outline outline;
    [SerializeField] private Color32 defaultOutlineColor = new Color32(25, 19, 17, 255);
    [SerializeField] private Color32 selectedOutlineColor = new Color32(40, 150, 44, 255);

    private SwarmUnitsData unitData;
    private Unit selectedUnit;
    private UnitInventoryUI inventoryUI;
    private UnitInventory currentInventory;

    private HealthManager healthManager;
    private EnergyManager energyManager;
    private MCFormController formController;
    private StatsManager statsManager;

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

        TryAssignUnit();
        UnitRegistry.OnUnitRegistered += OnUnitRegistered;

        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.OnSelectedUnitChanged += HandleSelectionChanged;
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
        selectedUnit = unit;
        statsManager = unit.GetComponent<StatsManager>();

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

        UpdateOutlineState();

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(UpdateStatsAfterInitialization());
        }
    }

    private IEnumerator UpdateStatsAfterInitialization()
    {
        yield return new WaitForEndOfFrame();

        HandleHealthChanged(healthManager != null ? (float)healthManager.CurrentHP / statsManager.MaxHP : 1f);
        HandleEnergyChanged(energyManager != null ? (float)energyManager.CurrentEnergy / statsManager.MaxEnergy : 1f);
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
        int maxHp = statsManager != null ? statsManager.MaxHP : unitData.unitType.maxHP;
        hpText.text = $"HP: {currentHp}/{maxHp}";
    }

    private void UpdateEnergyText(float currentEnergy, float percent)
    {
        int maxEnergy = statsManager != null ? statsManager.MaxEnergy : unitData.unitType.maxEnergy;
        int energyInt = Mathf.CeilToInt(currentEnergy);
        energyText.text = $"EN: {energyInt}/{maxEnergy}";
    }

    private void HandleInventoryChanged(object sender, EventArgs e)
    {
        RefreshInventoryVisuals();
    }

    private void HandleSelectionChanged(object sender, EventArgs e) => UpdateOutlineState();

    private void UpdateOutlineState()
    {
        if (outline == null || selectedUnit == null || UnitSelectionSystem.Instance == null)
            return;

        bool isSelected = UnitSelectionSystem.Instance.GetSelectedUnit() == selectedUnit;
        outline.effectColor = isSelected ? selectedOutlineColor : defaultOutlineColor;
    }

    private void RefreshInventoryVisuals()
    {
        if (currentInventory == null || currentInventory.InventorySO == null) return;

        int requiredSlots = currentInventory.InventorySO.maxCapacity;

        while (uiSlots.Count < requiredSlots)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, inventoryPanel);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            uiSlots.Add(slotUI);
            slotUI.ClearSlot();
        }

        while (uiSlots.Count > requiredSlots)
        {
            int lastIndex = uiSlots.Count - 1;
            Destroy(uiSlots[lastIndex].gameObject);
            uiSlots.RemoveAt(lastIndex);
        }

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

        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.OnSelectedUnitChanged -= HandleSelectionChanged;

        if (currentInventory != null && currentInventory.InventorySO != null)
        {
            currentInventory.InventorySO.OnInventoryChanged -= HandleInventoryChanged;
        }

        if (healthManager != null) healthManager.onHealthPercentChange -= HandleHealthChanged;
        if (energyManager != null) energyManager.onEnergyPercentChange -= HandleEnergyChanged;
        if (formController != null) formController.OnFormChanged -= HandleFormChanged;
    }
}