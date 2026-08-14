using System;
using UnityEngine;

[Serializable]
public class AddMaxEnergyPercentAction : EventAction
{
    public float percentage = 0.1f;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (selectedUnit == null || selectedUnit.unitType == null || !selectedUnit.isAlive)
            return;

        int bonus = Mathf.RoundToInt(selectedUnit.unitType.maxEnergy * percentage);
        selectedUnit.bonusMaxEnergy += bonus;
    }
}