using UnityEngine;

public class IdleState : UnitState
{
    public IdleState(UnitBase unit) : base(unit) { }

    public override void EnterState()
    {
        if(_agent.enabled)
            _agent.ResetPath();
        _unit.Animator.SetBool("IsWalking", false);
    }

    public override void Tick()
    {
        if (_unit.AttackTarget != null && _unit.Stats.PossibleAttacks.Count != 0)
        {
            Debug.Log("Switching to fight state");
            _unit.PrepareUnit();
            _unit.SetState(new FightState(_unit));
        }
    }

}
