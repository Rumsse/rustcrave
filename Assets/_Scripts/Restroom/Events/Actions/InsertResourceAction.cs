using UnityEngine;

[CreateAssetMenu(fileName = "RemoveResourceAction", menuName = "Restroom/Events/Actions/Remove Resource")]
public class InsertResourceAction : EventAction
{
    [SerializeField] ItemSO itemToRemove;
    [SerializeField] int amount = 1;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (itemToRemove == null || swarmState.GlobalInventory == null)
            return;

        swarmState.GlobalInventory.RemoveItem(itemToRemove, amount);
    }
}