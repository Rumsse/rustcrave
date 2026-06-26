using UnityEngine;

[CreateAssetMenu(fileName = "Melee Attack", menuName = "Attacks/Melee Attack")]
public class MeleeAttack : AttackBase
{
    [SerializeField] private bool skipFirst;
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        Debug.Log("Executing Melee Attack");

        if (!attacker || !target)
            return;
        
        base.Execute(target, attacker);
        
        if (skipFirst)
            skipFirst = false;
        else
            attacker.Animator?.Play(animationStateName);

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);
        target.HealthManager?.Damage(new DamageInfo(finalDamage, type, DeliveryMethod.Melee));
    }
}
