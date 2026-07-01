using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReceiveRandomResourcesAction", menuName = "Restroom/Events/Actions/Receive Random Resources")]
public class ReceiveRandomResourcesAction : EventAction
{
    [SerializeField] List<ItemSO> resourcePool = new();
    [SerializeField] int minAmount = 1;
    [SerializeField] int maxAmount = 4;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (resourcePool.Count == 0 || swarmState.GlobalInventory == null)
            return;

        int totalAmount = Random.Range(minAmount, maxAmount + 1);

        for (int i = 0; i < totalAmount; i++)
        {
            int randomIndex = Random.Range(0, resourcePool.Count);
            var item = resourcePool[randomIndex];

            if (item != null)
                swarmState.GlobalInventory.AddItem(item, 1);
        }
    }
}