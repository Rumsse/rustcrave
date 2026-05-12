using UnityEngine;

//special behavior for digging enemies to create a space between walking and idle states
public class DiggingState : UnitState
{
    private DiggingEnemyUnit _currentSpecialUnit;
    private float _duration;
    private float _currentPassed;
    private float _multiplier;
    
    public DiggingState(UnitBase unit) : base(unit)
    {
    }

    public override void EnterState()
    {
        _currentSpecialUnit = _unit as DiggingEnemyUnit;

        (_duration, _multiplier) = _currentSpecialUnit.GetDiggingDetails();
        _currentPassed = 0;

        //animation of digging

    }

    override public void Tick()
    {
        _currentPassed += Time.deltaTime * _multiplier;

        if (_currentPassed >= _duration)
            _unit.SetState(new IdleState(_unit));

    }

    public override void ExitState()
    {
        _currentSpecialUnit.ClearStolenItem();
        _currentSpecialUnit = null;
    }
}
