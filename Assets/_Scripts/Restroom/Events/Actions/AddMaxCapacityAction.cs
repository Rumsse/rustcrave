using System;

[Serializable]
public class AddMaxCapacityAction : EventAction
{
    public int capacityToAdd = 2;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (selectedUnit == null)
            return;

        selectedUnit.bonusCarryCapacity += capacityToAdd;
    }
}
