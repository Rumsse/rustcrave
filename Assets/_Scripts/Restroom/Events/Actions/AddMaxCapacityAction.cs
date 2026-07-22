using UnityEngine;

[CreateAssetMenu(fileName = "AddCapacityAction", menuName = "Restroom/Events/Actions/Add Capacity")]
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