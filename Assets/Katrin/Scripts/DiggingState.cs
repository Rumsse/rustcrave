using UnityEngine;

//special behavior for digging enemies to create a space between walking and idle states
public class DiggingState : UnitState
{
    private DiggingEnemyUnit _currentSpecialUnit;

    public DiggingState(UnitBase unit) : base(unit)
    {
    }

    //probably i need to add actual digging as it disappears too fast
    public override void EnterState()
    {
        Debug.Log("Digging...");

        _currentSpecialUnit = _unit as DiggingEnemyUnit;

        _unit.SetState(new IdleState(_unit));
    }

    public override void ExitState()
    {
        Debug.Log("Finished Digging");

        _currentSpecialUnit?.ClearStolenItem();
        _currentSpecialUnit = null;
    }

}
