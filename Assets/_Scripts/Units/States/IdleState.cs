using UnityEngine;

public class IdleState : UnitState
{
    public IdleState(UnitBase unit) : base(unit) { }

    public override void EnterState()
    {
        _agent.ResetPath();
        _unit.Animator.SetBool("IsWalking", false);
    }

    public override void Tick()
    {
        if (_unit.AttackTarget != null)
            _unit.SetState(new FightState(_unit));
    }
}
