using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class DamageSwarmAction : EventAction
{
    public int damageAmount = 2;
    public bool excludeMC = false;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        var units = swarmState.SwarmUnits.ToList();

        foreach (var unit in units)
        {
            if (!unit.isAlive)
                continue;

            if (excludeMC && unit.unitType != null && unit.unitType.unitType == UnitType.Conductor)
                continue;

            unit.currentHP -= damageAmount;

            if (unit.currentHP <= 0)
            {
                unit.currentHP = 0;
                swarmState.MarkDead(unit.id);
            }
        }
    }
}