using UnityEngine;

public class FightState : UnitState
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    public FightState(UnitBase unit) : base(unit) { }

    public override void Tick()
    {
        if (!_unit.CurrentAttack)
            return;

        if (_unit.IsPerformingSpecial)
        {
            if (_agent.enabled && _agent.hasPath)
                _agent.ResetPath();

            _unit.Animator.SetBool(IsWalkingHash, false);
            return;
        }

        if (!_unit.AttackTarget)
        {
            _unit.SetState(new IdleState(_unit));
            return;
        }

        Vector3 targetCenter = _unit.AttackTarget.transform.position;
        Vector3 closestEdge = targetCenter;
        Collider targetCollider = _unit.AttackTarget.GetComponentInChildren<Collider>();

        if (targetCollider != null)
        {
            closestEdge = targetCollider.ClosestPoint(_unit.transform.position);
            closestEdge.y = _unit.transform.position.y;
        }
        else if (_unit.AttackTarget.Agent != null)
        {
            Vector3 dir = (_unit.transform.position - targetCenter).normalized;
            closestEdge = targetCenter + dir * _unit.AttackTarget.Agent.radius;
        }

        float distToEdge = (_unit.transform.position - closestEdge).sqrMagnitude;
        float range = _unit.CurrentAttack.attackRange + _agent.radius;

        if (distToEdge <= range * range)
        {
            if (_agent.enabled && _agent.hasPath)
                _agent.ResetPath();

            _unit.Animator.SetBool(IsWalkingHash, false);
            _unit.TryAttack();
        }
        else
        {
            if (_agent.enabled && Vector3.SqrMagnitude(_agent.destination - closestEdge) > 0.1f)
                _agent.SetDestination(closestEdge);

            _unit.Animator.SetBool(IsWalkingHash, true);
        }
    }
}