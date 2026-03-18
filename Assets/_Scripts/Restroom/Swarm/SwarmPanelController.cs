using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SwarmPanelController : MonoBehaviour
{
    [SerializeField] SwarmState swarmState;
    [SerializeField] VisualTreeAsset unitSlotTemplate;

    VisualElement rootElement;
    VisualElement slotsContainer;

    readonly List<VisualElement> activeSlots = new();

    #region Initialization

    public void Initialize(VisualElement root, VisualElement swarmLayer)
    {
        rootElement = root;
        slotsContainer = root.Q<VisualElement>("swarm-slots-container");

        if (swarmState == null || slotsContainer == null || unitSlotTemplate == null)
            return;

        swarmState.OnSwarmChanged += RebuildSwarmUI;
        swarmState.OnUnitAdded += AddUnitSlot;

        RebuildSwarmUI();

        root.schedule.Execute(UpdateStats).Every(100);
    }

    void OnDestroy()
    {
        if (swarmState == null)
            return;

        swarmState.OnSwarmChanged -= RebuildSwarmUI;
        swarmState.OnUnitAdded -= AddUnitSlot;
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
            hpLabel.text = $"HP {unitData.currentHP}/{unitData.unitType.maxHP}";

        if (enLabel != null && unitData.unitType != null)
        {
            float energyPercent = (unitData.currentEnergy / unitData.unitType.maxEnergy) * 100f;
            enLabel.text = $"EN {Mathf.RoundToInt(energyPercent)}%";
        }
    }

    #endregion
}