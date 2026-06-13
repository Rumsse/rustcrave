using UnityEngine;

[CreateAssetMenu(fileName = "RestoreHealthAction", menuName = "Restroom/Events/Actions/Restore Health")]
public class RestoreHealthAction : EventAction
{
    [Range(0f, 1f)] public float percentage = 1.0f;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (selectedUnit == null || !selectedUnit.isAlive)
            return;

        int amountToRestore = Mathf.RoundToInt(selectedUnit.GetTotalMaxHP() * percentage);
        selectedUnit.RestoreHealth(amountToRestore);
    }
}