using UnityEngine;

[CreateAssetMenu(fileName = "Melee Attack", menuName = "Attacks/Melee Attack")]
public class MeleeAttack : AttackBase
{
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        Debug.Log("Executing Melee Attack");

        if (!attacker || !target)
            return;
        
        base.Execute(target, attacker);
        
        attacker.Animator?.Play(animationStateName);

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);
        target.HealthManager?.Damage(new DamageInfo(finalDamage, type, DeliveryMethod.Melee));
    }
}
