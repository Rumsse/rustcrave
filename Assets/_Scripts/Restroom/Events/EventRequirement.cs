using System;
using System.Linq;

[Serializable]
public abstract class EventRequirement
{
    public abstract bool IsMet(SwarmState swarmState);
}

[Serializable]
public class ItemRequirement : EventRequirement
{
    public ItemSO item;
    public int amount = 1;

    public override bool IsMet(SwarmState swarmState)
    {
        if (item == null || swarmState.GlobalInventory == null)
            return true;

        var slot = swarmState.GlobalInventory.inventoryItemList.FirstOrDefault(s => s.item == item);

        if (slot == null)
            return false;

        return slot.amount >= amount;
    }
}
