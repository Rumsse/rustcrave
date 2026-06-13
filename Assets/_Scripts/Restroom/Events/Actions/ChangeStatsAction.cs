using UnityEngine;

[CreateAssetMenu(fileName = "ChangeStatsAction", menuName = "Restroom/Events/Actions/Change Stats")]
public class ChangeStatsAction : EventAction
{
    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (selectedUnit == null || selectedUnit.unitType == null)
            return;

        int GetNewBonusInt(int baseValue, int currentBonus, int delta)
        {
            int change = Random.value > 0.5f ? delta : -delta;

            if (baseValue + currentBonus + change < 1)
                change = delta;

            return currentBonus + change;
        }

        float GetNewBonusFloat(float baseValue, float currentBonus, float delta)
        {
            float change = Random.value > 0.5f ? delta : -delta;

            if (baseValue + currentBonus + change < 1f)
                change = delta;

            return currentBonus + change;
        }

        selectedUnit.bonusMaxHP = GetNewBonusInt(selectedUnit.unitType.maxHP, selectedUnit.bonusMaxHP, 1);
        selectedUnit.bonusMaxEnergy = GetNewBonusInt(selectedUnit.unitType.maxEnergy, selectedUnit.bonusMaxEnergy, 10);
        selectedUnit.bonusDamage = GetNewBonusInt(selectedUnit.unitType.damage, selectedUnit.bonusDamage, 1);
        selectedUnit.bonusMiningPower = GetNewBonusFloat(selectedUnit.unitType.miningPower, selectedUnit.bonusMiningPower, 1f);
        selectedUnit.bonusSpeed = GetNewBonusFloat(selectedUnit.unitType.moveSpeed, selectedUnit.bonusSpeed, 1f);
        selectedUnit.bonusCarryCapacity = GetNewBonusInt(selectedUnit.unitType.carryCapacity, selectedUnit.bonusCarryCapacity, 1);

        selectedUnit.currentHP = Mathf.Clamp(selectedUnit.currentHP, 0, selectedUnit.GetTotalMaxHP());
        selectedUnit.currentEnergy = Mathf.Clamp(selectedUnit.currentEnergy, 0, selectedUnit.GetTotalMaxEnergy());
    }
}