using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReceiveRandomItemAction", menuName = "Restroom/Events/Actions/Receive Random Item")]
public class ReceiveRandomItemAction : EventAction
{
    [SerializeField] List<ItemSO> possibleItems = new();
    [SerializeField] int minItemsToDraw = 1;
    [SerializeField] int maxItemsToDraw = 1;
    [SerializeField] int amountPerItem = 1;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (possibleItems.Count == 0)
            return;

        int draws = Random.Range(minItemsToDraw, maxItemsToDraw + 1);

        for (int i = 0; i < draws; i++)
        {
            int randomIndex = Random.Range(0, possibleItems.Count);
            var itemToGive = possibleItems[randomIndex];

            if (itemToGive == null)
                continue;

            if (itemToGive is GadgetSO gadget)
            {
                if (swarmState.GlobalGadgetsInventory != null)
                    swarmState.GlobalGadgetsInventory.AddGadget(gadget);
            }
            else
            {
                if (swarmState.GlobalInventory != null)
                    swarmState.GlobalInventory.AddItem(itemToGive, amountPerItem);
            }
        }
    }
}