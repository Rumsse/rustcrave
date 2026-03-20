using UnityEngine;
using System.Collections.Generic;

public class UnitEquipment : MonoBehaviour
{
    [SerializeField] private int maxGadgetSlots = 2;
    [SerializeField] private List<GadgetSO> equippedGadgets = new List<GadgetSO>();

    private StatsManager statsManager;
    private Unit unit;
    private UnitInventory unitInventory;

    private void Awake()
    {
        statsManager = GetComponent<StatsManager>();
        unit = GetComponent<Unit>();
        unitInventory = GetComponent<UnitInventory>();
    }

    private void Start()
    {
        foreach (var gadget in equippedGadgets)
        {
            if (gadget != null)
            {
                statsManager.AddFlatStatModifier(gadget.modifiedStat, gadget.statIncreaseAmount);
            }
        }

        if (unit != null)
        {
            unit.RefreshStats();
        }

        if (unitInventory != null)
        {
            unitInventory.UpdateCapacity();
        }
    }

    public bool TryEquipGadget(GadgetSO gadget)
    {
        if (equippedGadgets.Count >= maxGadgetSlots)
        {
            return false;
        }

        equippedGadgets.Add(gadget);
        ApplyGadgetStats(gadget);
        return true;
    }

    public void UnequipGadget(GadgetSO gadget)
    {
        if (equippedGadgets.Remove(gadget))
        {
            RemoveGadgetStats(gadget);
        }
    }

    private void ApplyGadgetStats(GadgetSO gadget)
    {
        if (statsManager != null)
        {
            statsManager.AddFlatStatModifier(gadget.modifiedStat, gadget.statIncreaseAmount);
            if (unit != null) unit.RefreshStats();
            if (unitInventory != null) unitInventory.UpdateCapacity();
        }
    }

    private void RemoveGadgetStats(GadgetSO gadget)
    {
        if (statsManager != null)
        {
            statsManager.RemoveFlatStatModifier(gadget.modifiedStat, gadget.statIncreaseAmount);
            if (unit != null) unit.RefreshStats();
            if (unitInventory != null) unitInventory.UpdateCapacity();
        }
    }

    public List<GadgetSO> GetEquippedGadgets()
    {
        return equippedGadgets;
    }

    public int GetMaxSlots()
    {
        return maxGadgetSlots;
    }
}