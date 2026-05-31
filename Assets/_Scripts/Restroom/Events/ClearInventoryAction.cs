using UnityEngine;

[CreateAssetMenu(fileName = "ClearInventoryAction", menuName = "Restroom/Events/Actions/Clear Inventory")]
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