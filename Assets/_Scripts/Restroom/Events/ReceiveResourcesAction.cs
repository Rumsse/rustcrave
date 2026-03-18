using UnityEngine;

[CreateAssetMenu(fileName = "ReceiveResourcesAction", menuName = "Restroom/Events/Actions/Receive Resources")]
public class ReceiveResourcesAction : EventAction
{
    [SerializeField] OreSO Oretype;
    [SerializeField] int amount = 2;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (Oretype == null)
            return;

        if (swarmState.GlobalInventory == null)
            return;

        swarmState.GlobalInventory.AddItem(Oretype, amount);
    }
}
