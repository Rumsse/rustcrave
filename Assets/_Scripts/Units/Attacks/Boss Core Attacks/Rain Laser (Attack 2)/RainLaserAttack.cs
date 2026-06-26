using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "Rain Laser", menuName = "Attacks/Boss Core/Rain Laser")]
public class RainLaserAttack : AttackBase
{
    [Header("Rain Laser")]
    public int hitAmount = 5;
    public float attackDelay = .2f;
    public float timeToFire = 1.5f;
    public float possibleHitRange = 15f;
    public int attacksAtUnit = 2;
    public LayerMask mask;

    [Header("Tween Settings")]
    public TweenSettings indicatorScaleSettings;
    public TweenSettings laserScaleSettings;

    [Header("Color Settings")]
    [ColorUsage(true, true)] public Color startColor;
    [ColorUsage(true, true)] public Color endColor;
    public TweenSettings colorTweenSettings;

    [Header("References")]
    public RainLaserController controllerPrefab;
    public Laser laserPrefab;

    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker)
            return;

        attacker.Animator?.Play(animationStateName);
        attacker.IsPerformingSpecial = true;

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);

        RainLaserController controller = PoolManager.Instance.Get(controllerPrefab);
        controller.transform.position = attacker.transform.position;
        controller.transform.rotation = Quaternion.identity;
        controller.Init(new DamageInfo(finalDamage, type), this, attacker, effects);
    }
}