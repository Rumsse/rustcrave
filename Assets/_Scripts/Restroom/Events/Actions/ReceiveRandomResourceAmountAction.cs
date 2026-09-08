using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ReceiveRandomResourceAmountAction : EventAction
{
    [SerializeField] ItemData itemToAdd;
    [SerializeField] int minAmount = 4;
    [SerializeField] int maxAmount = 6;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (itemToAdd == null || swarmState.GlobalInventory == null)
            return;

        int amount = Random.Range(minAmount, maxAmount + 1);
        swarmState.GlobalInventory.AddItem(itemToAdd, amount);
    }
}