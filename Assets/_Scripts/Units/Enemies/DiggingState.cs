using UnityEngine;

//special behavior for digging enemies to create a space between walking and idle states
public class DiggingState : UnitState
{
    private DiggingEnemyUnit _currentSpecialUnit;

    public DiggingState(UnitBase unit) : base(unit)
    {
    }

    public override void EnterState()
    {
        _currentSpecialUnit = _unit as DiggingEnemyUnit;

        _unit.SetState(new IdleState(_unit));
    }

    public override void ExitState()
    {
        _currentSpecialUnit?.ClearStolenItem();
        _currentSpecialUnit = null;
    }

}
