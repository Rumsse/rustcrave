using UnityEngine;

[CreateAssetMenu(fileName = "InsertResourceAction", menuName = "Restroom/Events/Actions/Insert Resource")]
public class InsertResourceAction : EventAction
{
    [SerializeField] ItemSO itemToInsert;
    [SerializeField] int amount = 1;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (itemToInsert == null || swarmState.GlobalInventory == null)
            return;

        swarmState.GlobalInventory.AddItem(itemToInsert, amount);
    }
}