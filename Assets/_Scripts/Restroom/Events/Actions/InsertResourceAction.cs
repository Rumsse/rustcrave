using System;
using UnityEngine;

[Serializable]
public class InsertResourceAction : EventAction
{
    [SerializeField] ItemData itemToInsert;
    [SerializeField] int amount = 1;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (itemToInsert == null || swarmState.GlobalInventory == null)
            return;

        swarmState.GlobalInventory.AddItem(itemToInsert, amount);
    }
}