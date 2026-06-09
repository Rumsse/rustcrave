using System;
using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "Target Laser", menuName = "Attacks/Boss Core/Target Laser")]
public class TargetLaserAttack : AttackBase
{
    [Header("Target Laser")]
    public float timeToFire = 1.5f;
    public LayerMask mask;

    [Header("Tween Settings")]
    public TweenSettings indicatorScaleSettings;
    public TweenSettings laserScaleSettings;

    [Header("Color Settings")]
    [ColorUsage(true, true)] public Color startColor;
    [ColorUsage(true, true)] public Color endColor;
    public TweenSettings colorTweenSettings;

    [Header("References")]
    public TargetLaserController controllerPrefab;

    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!attacker)
            return;

        Debug.Log($"[TargetLaserAttack] Executing attack for {attacker.name}. Attempting to pause movement.");

        attacker.IsPerformingSpecial = true;

        if (!string.IsNullOrEmpty(animationStateName) && attacker.Animator != null && attacker.Animator.runtimeAnimatorController != null)
            attacker.Animator.Play(animationStateName);

        var spawner = attacker.GetComponent<UnitSpawnOnTransformList>();
        if (spawner)
            spawner.enabled = false;

        var unitController = attacker.GetComponent<UnitController>();
        if (!unitController) unitController = attacker.GetComponentInChildren<UnitController>(true);
        if (!unitController) unitController = attacker.transform.root.GetComponentInChildren<UnitController>(true);

        if (unitController)
        {
            unitController.PauseForSpecialAttack();
        }
        else
        {
            Debug.LogWarning($"[TargetLaserAttack] UnitController still not found on {attacker.name}! Using raw Agent fallback stop.");

            if (attacker.Agent && attacker.Agent.isActiveAndEnabled)
            {
                attacker.Agent.ResetPath();
                attacker.Agent.velocity = Vector3.zero;
                attacker.Agent.isStopped = true;
            }
        }

        int finalDamage = GetFinalDamage(attacker.Stats.Damage);

        TargetLaserController controller = PoolManager.Instance.Get(controllerPrefab);
        controller.transform.position = attacker.transform.position;
        controller.transform.rotation = Quaternion.identity;

        controller.Init(new DamageInfo(finalDamage, type), this, attacker, effects, () => RestoreUnitMovement(attacker, unitController));
    }

    private void RestoreUnitMovement(UnitBase attacker, UnitController unitController)
    {
        Debug.Log($"[TargetLaserAttack] Attack sequence complete for {attacker?.name}. Restoring movement.");

        if (attacker)
        {
            attacker.IsPerformingSpecial = false;

            var spawner = attacker.GetComponent<UnitSpawnOnTransformList>();
            if (spawner)
                spawner.enabled = true;
        }

        if (unitController)
        {
            unitController.ResumeFromSpecialAttack();
        }
        else if (attacker && attacker.Agent && attacker.Agent.isActiveAndEnabled)
        {
            attacker.Agent.isStopped = false;
        }
    }
}