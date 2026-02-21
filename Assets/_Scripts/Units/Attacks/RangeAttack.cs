using UnityEngine;

[CreateAssetMenu(fileName = "Range Attack", menuName = "Attacks/Range Attack")]
public class RangeAttack : AttackBase
{
    public Projectile projectilePrefab;
    public float projectileSpeed;
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker || !target)
            return;
        
        attacker.Animator?.Play(animationStateName);

        int finalDamage = GetFinalDamage(attacker.Stats.damage);
        
        Projectile proj = PoolManager.Instance.Get(projectilePrefab);
        proj.transform.position = attacker.ProjectileSpawnT.position;
        proj.transform.rotation = Quaternion.LookRotation(target.transform.position - attacker.transform.position);
        proj.Init(finalDamage, projectileSpeed, target, projectilePrefab);
        // Damage is applied on projectile script
    }
}
