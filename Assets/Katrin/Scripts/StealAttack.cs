using UnityEngine;

[CreateAssetMenu(fileName = "Steal Attack", menuName = "Attacks/Steal Attack")]
public class StealAttack : AttackBase // logic for stealing random item from the unit
{
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        Debug.Log("Executing Steal Attack");

        if (!attacker || !target)
            return;

        base.Execute(target, attacker);

        attacker.Animator?.Play(animationStateName);

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);
        target.HealthManager?.Damage(new DamageInfo(finalDamage, type, DeliveryMethod.Melee));

        // Steal a random item from the target unit and give it to the attacker unit
        Unit unitTarget = target as Unit;
        EnemyUnit attackerUnit = attacker as EnemyUnit;

        ItemSO stolenItem = unitTarget?.Inventory.InventorySO.StealRandomItem();
        attackerUnit?.StealItem(stolenItem);

    }

}
