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
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private GameObject inventorySlotPrefab;

    [Header("Selection Visuals")]
    [SerializeField] private Outline outline;
    [SerializeField] private Color32 defaultOutlineColor = new Color32(25, 19, 17, 255);
    [SerializeField] private Color32 selectedOutlineColor = new Color32(40, 150, 44, 255);
    [SerializeField] private Image selectionBackground;

    [Header("Bar Settings")]
    [SerializeField] private float maxFillLimit = 0.25f;
    [SerializeField] private float minVisibleFillOffset = 0.03f;

    [Header("Circular Inventory")]
    [SerializeField] private CircularInventorySlot circularSlotPrefab;
    [SerializeField] private Transform circularInventoryContainer;
    [SerializeField] private float slotGap = 0.01f;
    [SerializeField] private float startAngleOffset = 0f;

    private SwarmUnitsData unitData;
    private Unit selectedUnit;
    private UnitInventoryUI inventoryUI;
    private UnitInventory currentInventory;

    private HealthManager healthManager;
    private EnergyManager energyManager;
    private MCFormController formController;
    private StatsManager statsManager;

    private int lastDisplayedHP = -1;
    private int lastDisplayedEnergy = -1;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private List<CircularInventorySlot> circularSlots = new List<CircularInventorySlot>();


    public void OnClickIcon()
    {
        if (selectedUnit == null)
            return;

        UnitPanel.Instance.TogglePanel(selectedUnit);

        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.SetSelectedUnit(selectedUnit);
    }

    public void Setup(SwarmUnitsData data)
    {
        unitData = data;

        nameText.text = unitData.unitType.robotName;
        icon.sprite = unitData.unitType.robotSprite;

        lastDisplayedHP = -1;
        lastDisplayedEnergy = -1;

        UpdateHealthVisuals(unitData.currentHP, (float)unitData.currentHP / unitData.unitType.maxHP);
        UpdateEnergyVisuals(unitData.currentEnergy, (float)unitData.currentEnergy / unitData.unitType.maxEnergy);

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

        UpdateSelectionState();
        //UpdateOutlineState();

        if (gameObject.activeInHierarchy)
        {
            UpdateStatsAfterInitialization();
        }
    }

    private async void UpdateStatsAfterInitialization()
    {
        await Awaitable.EndOfFrameAsync();

        HandleHealthChanged(healthManager != null ? (float)healthManager.CurrentHP / statsManager.MaxHP : 1f);
        HandleEnergyChanged(energyManager != null ? (float)energyManager.CurrentEnergy / statsManager.MaxEnergy : 1f);
    }

    private void UpdateHealthVisuals(int currentHp, float percent)
    {
        if (currentHp != lastDisplayedHP)
        {
            lastDisplayedHP = currentHp;
            int maxHp = statsManager != null ? statsManager.MaxHP : unitData.unitType.maxHP;
            hpText.text = $"HP: {currentHp}/{maxHp}";
        }

        if (hpFillImage != null)
            hpFillImage.fillAmount = percent > 0f ? Mathf.Lerp(minVisibleFillOffset, maxFillLimit, percent) : 0f;
    }

    private void UpdateEnergyVisuals(float currentEnergy, float percent)
    {
        int energyInt = Mathf.CeilToInt(currentEnergy);

        if (energyInt != lastDisplayedEnergy)
        {
            lastDisplayedEnergy = energyInt;
            int maxEnergy = statsManager != null ? statsManager.MaxEnergy : unitData.unitType.maxEnergy;
            energyText.text = $"EN: {energyInt}/{maxEnergy}";
        }

        if (energyFillImage != null)
            energyFillImage.fillAmount = percent > 0f ? Mathf.Lerp(minVisibleFillOffset, maxFillLimit, percent) : 0f;
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
        UpdateHealthVisuals(healthManager.CurrentHP, percent);
    }

    private void HandleEnergyChanged(float percent)
    {
        if (energyManager == null || unitData == null) return;
        UpdateEnergyText(energyManager.CurrentEnergy, percent);
        UpdateEnergyVisuals(energyManager.CurrentEnergy, percent);
    }

    private void UpdateHealthText(int currentHp, float percent)
    {
        if (currentHp == lastDisplayedHP) return;

        lastDisplayedHP = currentHp;
        int maxHp = statsManager != null ? statsManager.MaxHP : unitData.unitType.maxHP;
        hpText.text = $"HP: {currentHp}/{maxHp}";
    }

    private void UpdateEnergyText(float currentEnergy, float percent)
    {
        int energyInt = Mathf.CeilToInt(currentEnergy);

        if (energyInt == lastDisplayedEnergy) return;

        lastDisplayedEnergy = energyInt;
        int maxEnergy = statsManager != null ? statsManager.MaxEnergy : unitData.unitType.maxEnergy;
        energyText.text = $"EN: {energyInt}/{maxEnergy}";
    }

    private void HandleInventoryChanged(object sender, EventArgs e)
    {
        RefreshInventoryVisuals();
    }

    private void HandleSelectionChanged(object sender, EventArgs e) => UpdateSelectionState();

    private void UpdateOutlineState()
    {
        if (outline == null || selectedUnit == null || UnitSelectionSystem.Instance == null)
            return;

        bool isSelected = UnitSelectionSystem.Instance.GetSelectedUnit() == selectedUnit;
        outline.effectColor = isSelected ? selectedOutlineColor : defaultOutlineColor;
    }

    private void UpdateSelectionState()
    {
        if (selectionBackground == null || selectedUnit == null || UnitSelectionSystem.Instance == null)
            return;

        bool isSelected = UnitSelectionSystem.Instance.GetSelectedUnit() == selectedUnit;

        if (selectionBackground.enabled != isSelected)
            selectionBackground.enabled = isSelected;
    }

    private void RefreshInventoryVisuals()
    {
        if (currentInventory == null || currentInventory.InventorySO == null)
            return;

        int requiredSlots = currentInventory.InventorySO.maxCapacity;

        while (circularSlots.Count < requiredSlots)
        {
            CircularInventorySlot slot = Instantiate(circularSlotPrefab, circularInventoryContainer);
            circularSlots.Add(slot);
        }

        while (circularSlots.Count > requiredSlots)
        {
            int lastIndex = circularSlots.Count - 1;
            Destroy(circularSlots[lastIndex].gameObject);
            circularSlots.RemoveAt(lastIndex);
        }

        float fillPerSlot = (1f / requiredSlots) - slotGap;
        float anglePerSlot = 360f / requiredSlots;

        for (int i = 0; i < circularSlots.Count; i++)
        {
            float angle = startAngleOffset + (i * anglePerSlot);
            circularSlots[i].Setup(fillPerSlot, angle);
        }

        int currentVisualSlotIndex = 0;

        foreach (var inventorySlot in currentInventory.InventorySO.inventoryItemList)
        {
            for (int i = 0; i < inventorySlot.amount; i++)
            {
                if (currentVisualSlotIndex < circularSlots.Count)
                {
                    circularSlots[currentVisualSlotIndex].SetItem(inventorySlot.item.itemColor);
                    currentVisualSlotIndex++;
                }
            }
        }

        for (int i = currentVisualSlotIndex; i < circularSlots.Count; i++)
            circularSlots[i].SetEmpty();
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