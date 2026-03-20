using UnityEngine;

[CreateAssetMenu(fileName = "DamageAction", menuName = "Restroom/Events/Actions/Take Damage")]
public class DamageEventAction : EventAction
{
    [SerializeField] public int damageAmount = 10;

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
}