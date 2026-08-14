using System;
using UnityEngine;

[Serializable]
public class DamageEventAction : EventAction
{
    [SerializeField] public int damageAmount = 10;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (!selectedUnit.isAlive)
            return;

        selectedUnit.currentHP -= damageAmount;

        if (selectedUnit.currentHP > 0)
            return;

        selectedUnit.currentHP = 0;
        swarmState.MarkDead(selectedUnit.id);
    }
}