using UnityEngine.AI;

public class UnitState
{
    protected UnitBase _unit;
    protected NavMeshAgent _agent;

    public UnitState(UnitBase unit)
    {
        _unit = unit;
        _agent = unit.GetComponent<NavMeshAgent>();
    }
    
    public virtual void EnterState(){}
    public virtual void Tick() {}
    public virtual void ExitState(){}
}
