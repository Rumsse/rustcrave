using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class MimicryUnitDetection : PlayerUnitDetector
{
    [SerializeField] private float pushForce;
    [SerializeField] private int extraDamage;

    private float radius;
    private Unit targetUnit;

    private void Start()
    {
        radius = GetComponent<SphereCollider>().radius;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!unit) return;

        if (!other.CompareTag("Unit"))
            return;

        if (!other.TryGetComponent(out Unit unitObj))
            return;

        unit.UnitEnter(unitObj);

        targetUnit = unitObj;
        PushUnit(); 
    }

    private void PushUnit()
    {
        if(Vector3.Distance(transform.position, targetUnit.transform.position) > radius)
            return;

        Vector3 pushDir = -unit.transform.forward;
        Vector3 moveDelta = pushDir.normalized * pushForce;

        targetUnit.TryGetComponent(out HealthManager targetHealth);
        targetUnit.TryGetComponent(out NavMeshAgent targetAgent);

        targetAgent?.Move(moveDelta);
        targetHealth?.Damage(new DamageInfo(extraDamage, AttackType.Physical, DeliveryMethod.Melee));

    }

    protected override void OnTriggerExit(Collider other)
    {
        targetUnit = null;
    }
}
