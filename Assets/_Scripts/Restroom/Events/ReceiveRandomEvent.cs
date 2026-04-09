using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReceiveRandomItemAction", menuName = "Restroom/Events/Actions/Receive Random Item")]
public class ReceiveRandomItemAction : EventAction
{
    [SerializeField] List<ItemSO> possibleItems = new();
    [SerializeField] int amount = 1;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (possibleItems.Count == 0 || swarmState.GlobalGadgetsInventory == null)
            return;

        int randomIndex = Random.Range(0, possibleItems.Count);
        var itemToGive = possibleItems[randomIndex];

        if (itemToGive != null && itemToGive is GadgetSO gadget)
            swarmState.GlobalGadgetsInventory.AddGadget(gadget);

        if (itemToGive != null && itemToGive is not GadgetSO)
            swarmState.GlobalInventory.AddItem(itemToGive, amount);
    }
}