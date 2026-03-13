using UnityEngine;

public abstract class EventAction : ScriptableObject
{
    public abstract void Execute(SwarmUnitsData selectedUnit);
}

[CreateAssetMenu(fileName = "StandardAction", menuName = "Restroom/Events/Standard Action")]
public class StandardEventAction : EventAction
{
    public override void Execute(SwarmUnitsData selectedUnit)
    {
    }
}

[CreateAssetMenu(fileName = "DamageAction", menuName = "Restroom/Events/Damage Action")]
public class DamageEventAction : EventAction
{
    public int damageAmount = 10;

    public override void Execute(SwarmUnitsData selectedUnit)
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
}