using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "Target Laser", menuName = "Attacks/Boss Core/Target Laser")]
public class TargetLaserAttack : AttackBase
{
    [Header("Target Laser")]
    [Tooltip("Time it takes to fire after indicator shows up")]
    public float timeToFire = 1.5f;
    public LayerMask mask;
    
    [Header("Tween Settings")]
    public TweenSettings indicatorScaleSettings;
    public TweenSettings laserScaleSettings;
    public TweenSettings<Color> colorSettings;
    
    [Header("References")]
    public TargetLaserController controllerPrefab;
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker)
            return;
        
        attacker.Animator?.Play(animationStateName);
        attacker.IsPerformingSpecial = true;

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);

        TargetLaserController controller = PoolManager.Instance.Get(controllerPrefab);
        controller.transform.position = attacker.transform.position;
        controller.transform.rotation = Quaternion.identity;
        controller.Init(new DamageInfo(finalDamage, type), this, attacker, effects);
    }
}
