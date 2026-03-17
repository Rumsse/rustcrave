using UnityEngine;

[CreateAssetMenu(fileName = "AOE Attack", menuName = "Attacks/AOE Attack")]
public class AOEAttack : AttackBase
{
    public AOEProjectile projectilePrefab;
    public bool spawnOnAttacker;
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker || !target)
            return;
        
        attacker.Animator?.Play(animationStateName);
        
        int finalDamage = GetFinalDamage(attacker.Stats.Damage);
        
        Vector3 targetPos = spawnOnAttacker ? attacker.transform.position : target.transform.position;
        
        AOEProjectile proj = PoolManager.Instance.Get(projectilePrefab);
        proj.transform.position = targetPos;
        proj.transform.rotation = Quaternion.LookRotation(target.transform.position - attacker.transform.position);
        proj.Init(new DamageInfo(finalDamage, type, DeliveryMethod.AOE), projectilePrefab, effects);
        //Damage is applied on projectiles script
    }
}
