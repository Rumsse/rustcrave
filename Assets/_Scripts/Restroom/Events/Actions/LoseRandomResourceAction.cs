using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class LoseRandomResourceAction : EventAction
{
    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (swarmState.GlobalInventory == null)
            return;

        var items = swarmState.GlobalInventory.inventoryItemList;

        if (items.Count == 0)
            return;

        int index = Random.Range(0, items.Count);
        var itemToRemove = items[index].item;
        var amount = items[index].amount;

        swarmState.GlobalInventory.RemoveItem(itemToRemove, amount);
    }
}