using UnityEngine;

[CreateAssetMenu(fileName = "Melee Attack", menuName = "Attacks/Melee Attack")]
public class MeleeAttack : AttackBase
{
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker || !target)
            return;
        
        attacker.Animator?.Play(animationStateName);

        int finalDamage = Mathf.RoundToInt(damageMult * attacker.Stats.damage);
        target.HealthManager?.Damage(finalDamage);
    }
}
