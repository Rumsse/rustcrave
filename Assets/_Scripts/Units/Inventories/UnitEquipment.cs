using UnityEngine;
using System.Collections.Generic;

public class UnitEquipment : MonoBehaviour
{
    [SerializeField] private int maxGadgetSlots = 2;
    [SerializeField] private List<GadgetData> equippedGadgets = new List<GadgetData>();

    private StatsManager statsManager;
    private Unit unit;
    private UnitInventory unitInventory;

    private void Awake()
    {
        statsManager = GetComponent<StatsManager>();
        unit = GetComponent<Unit>();
        unitInventory = GetComponent<UnitInventory>();
    }

    public void Initialize(List<GadgetData> savedGadgets)
    {
        foreach (var gadget in equippedGadgets)
            RemoveGadgetStats(gadget);

        equippedGadgets.Clear();

        foreach (var gadget in savedGadgets)
        {
            if (gadget == null) continue;

            equippedGadgets.Add(gadget);
            ApplyGadgetStats(gadget);
        }
    }

    /*    private void Start()
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
        }*/

    public bool TryEquipGadget(GadgetData gadget)
    {
        if (equippedGadgets.Count >= maxGadgetSlots)
        {
            return false;
        }

        equippedGadgets.Add(gadget);
        ApplyGadgetStats(gadget);
        return true;
    }

    public void UnequipGadget(GadgetData gadget)
    {
        if (equippedGadgets.Remove(gadget))
        {
            RemoveGadgetStats(gadget);
        }
    }

    private void ApplyGadgetStats(GadgetData gadget)
    {
        if (statsManager != null)
        {
            statsManager.AddFlatStatModifier(gadget.modifiedStat, gadget.statIncreaseAmount);
            if (unit != null) unit.RefreshStats();
            if (unitInventory != null) unitInventory.UpdateCapacity();
        }
    }

    private void RemoveGadgetStats(GadgetData gadget)
    {
        if (statsManager != null)
        {
            statsManager.RemoveFlatStatModifier(gadget.modifiedStat, gadget.statIncreaseAmount);
            if (unit != null) unit.RefreshStats();
            if (unitInventory != null) unitInventory.UpdateCapacity();
        }
    }

    public List<GadgetData> GetEquippedGadgets()
    {
        return equippedGadgets;
    }

    public int GetMaxSlots()
    {
        return maxGadgetSlots;
    }
}