using System;
using UnityEngine;

[Serializable]
public class CloneUnitAction : EventAction
{
    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (selectedUnit == null || selectedUnit.unitType == null || swarmState == null)
            return;

        if (swarmState.AliveCount >= swarmState.MaxSwarmSize)
            return;

        swarmState.AddUnitToSwarm(selectedUnit.unitType);
    }
}