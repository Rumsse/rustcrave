using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class DoubleInventoryAction : EventAction
{
    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (swarmState.GlobalInventory == null)
            return;

        var currentItems = swarmState.GlobalInventory.inventoryItemList.ToList();

        foreach (var slot in currentItems)
        {
            swarmState.GlobalInventory.AddItem(slot.item, slot.amount);
        }
    }
}