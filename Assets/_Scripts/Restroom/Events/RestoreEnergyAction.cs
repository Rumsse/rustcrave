using UnityEngine;

[CreateAssetMenu(fileName = "RestoreEnergyAction", menuName = "Restroom/Events/Actions/Restore Energy")]
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
}