using UnityEngine;

public class FightState : UnitState
{
    public FightState(UnitBase unit) : base(unit) {}

    public override void Tick()
    {
        if (!_unit.AttackTarget)
        {
            _unit.SetState(new IdleState(_unit));
            return;
        }
        
        float dist = (_unit.transform.position - _unit.AttackTarget.transform.position).sqrMagnitude;
        float range = _unit.CurrentAttack.attackRange + _agent.radius;
        range *= range; // sqr to math sqrMagnitude

        if (dist < range)
        {
            _agent.ResetPath();
            _unit.Animator.SetBool("IsWalking", false);
            _unit.TryAttack();
        } 
        else
        {
            _agent.SetDestination(_unit.AttackTarget.transform.position);
            _unit.Animator.SetBool("IsWalking", true);
        }
    }
}
