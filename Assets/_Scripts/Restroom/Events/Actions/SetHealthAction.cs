using System;
using UnityEngine;

[Serializable]
public class SetHealthAction : EventAction
{
    public int targetHealth = 1;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (selectedUnit == null || !selectedUnit.isAlive)
            return;

        selectedUnit.currentHP = Mathf.Clamp(targetHealth, 1, selectedUnit.GetTotalMaxHP());
    }
}