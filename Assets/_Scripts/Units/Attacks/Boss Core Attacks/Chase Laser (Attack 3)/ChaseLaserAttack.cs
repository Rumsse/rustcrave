using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase Laser", menuName = "Attacks/Boss Core/Chase Laser")]
public class ChaseLaserAttack : AttackBase
{
    [Header("Chase Laser")]
    public float rotateAmount = 180f;
    public float timeToFire = 1.5f;

    [Header("Tween Settings")]
    public TweenSettings indicatorScaleSettings;
    public TweenSettings laserScaleSettings;
    public TweenSettings rotationSettings;

    [Header("Color Settings")]
    [ColorUsage(true, true)] public Color startColor;
    [ColorUsage(true, true)] public Color endColor;
    public TweenSettings colorTweenSettings;

    [Header("References")]
    public ChaseLaserController controllerPrefab;

    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker)
            return;

        attacker.Animator?.Play(animationStateName);
        attacker.IsPerformingSpecial = true;

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);

        ChaseLaserController controller = PoolManager.Instance.Get(controllerPrefab);
        controller.transform.position = attacker.transform.position;
        controller.transform.rotation = Quaternion.identity;
        controller.Init(new DamageInfo(finalDamage, type), this, attacker, effects);
    }
}