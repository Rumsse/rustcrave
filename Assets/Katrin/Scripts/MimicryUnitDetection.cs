using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class MimicryUnitDetection : PlayerUnitDetector
{
    [SerializeField] private float pushForce;
    [SerializeField] private int extraDamage;

    private float _radius;
    private Unit _targetUnit;

    private void Start()
    {
        _radius = GetComponent<SphereCollider>().radius;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!unit || unit.FarDetectEnabled())
            return;

        if (!other.CompareTag("Unit"))
            return;

        if (!other.TryGetComponent(out Unit unitObj))
            return;

        unit.UnitEnter(unitObj);

        if (unit is MimicryEnemy mimic)
            mimic.PrepareUnit();

        _targetUnit = unitObj;
        PushUnit();
    }

    protected override void OnTriggerExit(Collider other)
    {
        _targetUnit = null;
    }

    #region Combat Logic

    private void PushUnit()
    {
        if (Vector3.Distance(transform.position, _targetUnit.transform.position) > _radius)
            return;

        Vector3 pushDir = -unit.transform.forward.normalized;

        _targetUnit.TryGetComponent(out HealthManager targetHealth);
        _targetUnit.TryGetComponent(out IPushable targetAgent);

        targetAgent?.ApplyPush(pushDir, pushForce);
        targetHealth?.Damage(new DamageInfo(extraDamage, AttackType.Physical, DeliveryMethod.Melee));
    }

    #endregion
}