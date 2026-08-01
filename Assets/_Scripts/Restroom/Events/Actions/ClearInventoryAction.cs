using System;
using UnityEngine;

[Serializable]
public class ClearInventoryAction : EventAction
{
    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (swarmState.GlobalInventory == null)
            return;

        swarmState.GlobalInventory.Reset();
        swarmState.GlobalInventory.ForceRefresh();
    }
}