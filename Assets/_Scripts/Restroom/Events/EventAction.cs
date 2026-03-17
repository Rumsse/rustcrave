using UnityEngine;

public abstract class EventAction : ScriptableObject
{
    public abstract void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState);
}

/*[CreateAssetMenu(fileName = "RestoreEnergyAction", menuName = "Restroom/Events/Actions/Restore Energy")]
public class RestoreEnergyAction : EventAction
{
    public float percentage = 1.0f;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (selectedUnit.unitType == null)
            return;

        float amount = selectedUnit.unitType.maxEnergy * percentage;
        selectedUnit.RestoreEnergy(amount);
    }
}*/

/*[CreateAssetMenu(fileName = "RestoreSwarmEnergyAction", menuName = "Restroom/Events/Actions/Restore Swarm Energy")]
public class RestoreSwarmEnergyAction : EventAction
{
    public float percentage = 0.1f;

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
}*/

/*[CreateAssetMenu(fileName = "DamageAction", menuName = "Restroom/Events/Actions/Take Damage")]
public class DamageEventAction : EventAction
{
    public int damageAmount = 10;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (!selectedUnit.isAlive)
            return;

        selectedUnit.currentHP -= damageAmount;

        if (selectedUnit.currentHP <= 0)
        {
            selectedUnit.currentHP = 0;
            selectedUnit.isAlive = false;
        }
    }
}*/