using System;
using UnityEngine;

[Serializable]
public class RestoreSwarmEnergyAction : EventAction
{
    [SerializeField] float percentage = 0.1f;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        foreach (var unit in swarmState.SwarmUnits)
        {
            if (!unit.isAlive || unit.unitType == null)
                continue;

            float amount = unit.unitType.maxEnergy * percentage;
            unit.RestoreEnergy(amount);
        }
    }
}