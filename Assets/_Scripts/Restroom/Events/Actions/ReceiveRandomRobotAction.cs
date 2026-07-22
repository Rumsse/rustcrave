using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ReceiveRandomRobotAction", menuName = "Restroom/Events/Actions/Receive Random Robot")]
public class ReceiveRandomRobotAction : EventAction
{
    [SerializeField] List<UnitSO> possibleRobots = new();
    [SerializeField] int startingHP = 2;
    [SerializeField, Range(0f, 1f)] float startingEnergyPercent = 0.4f;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (possibleRobots.Count == 0 || swarmState == null)
            return;

        if (swarmState.AliveCount >= swarmState.MaxSwarmSize)
            return;

        int randomIndex = Random.Range(0, possibleRobots.Count);
        var robotToGive = possibleRobots[randomIndex];

        if (robotToGive == null)
            return;

        int initialCount = swarmState.SwarmUnits.Count;

        swarmState.AddUnitToSwarm(robotToGive);

        if (swarmState.SwarmUnits.Count > initialCount)
        {
            var newUnit = swarmState.SwarmUnits.Last();
            newUnit.currentHP = Mathf.Clamp(startingHP, 1, newUnit.GetTotalMaxHP());
            newUnit.currentEnergy = Mathf.Clamp(newUnit.GetTotalMaxEnergy() * startingEnergyPercent, 0, newUnit.GetTotalMaxEnergy());
        }
    }
}