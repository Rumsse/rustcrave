using UnityEngine;

public class WalkingState : UnitState
{
    private Vector3 _targetPos;
    
    public WalkingState(UnitBase unit, Vector3 pos) : base(unit) => _targetPos = pos;

    public override void EnterState()
    {
        if(_agent.enabled)
            _agent.SetDestination(_targetPos);
        _unit.Animator.SetBool("IsWalking", true);
    }

    override public void Tick()
    {
        if(_agent.enabled && _agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending)
            _unit.SetState(new IdleState(_unit));
    }
}
