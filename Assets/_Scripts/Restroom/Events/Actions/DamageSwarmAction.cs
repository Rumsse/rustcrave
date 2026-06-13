using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageSwarmAction", menuName = "Restroom/Events/Actions/Damage Swarm")]
public class DamageSwarmAction : EventAction
{
    public int damageAmount = 2;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        var units = swarmState.SwarmUnits.ToList();

        foreach (var unit in units)
        {
            if (!unit.isAlive)
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