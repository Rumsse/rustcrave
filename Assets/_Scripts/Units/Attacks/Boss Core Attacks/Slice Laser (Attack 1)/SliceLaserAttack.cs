using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Slice Laser", menuName = "Attacks/Boss Core/Slice Laser")]
public class SliceLaserAttack : AttackBase
{
    [Header("Slice Laser")] 
    public int sliceAmount = 3;
    public float recursionDelay = .2f;
    public LayerMask mask;
    public float delayAfterStopping = .5f;
    
    [Header("Rotation Settings")]
    public float rotationMin;
    public float rotationMax;

    [Header("Tween Settings")] 
    public TweenSettings rotationSettings;
    [FormerlySerializedAs("scaleSettings")] public TweenSettings indicatorScaleSettings;
    public TweenSettings laserScaleSettings;
    public TweenSettings<Color> colorSettings;
    
    [Header("References")]
    public SliceLaserController controllerPrefab;
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker)
            return;
        
        attacker.Animator?.Play(animationStateName);
        attacker.IsPerformingSpecial = true;

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);

        SliceLaserController controller = PoolManager.Instance.Get(controllerPrefab);
        controller.transform.position = attacker.transform.position;
        controller.transform.rotation = Quaternion.identity;
        controller.Init(new DamageInfo(finalDamage, type), this, attacker, effects);
    }
}
