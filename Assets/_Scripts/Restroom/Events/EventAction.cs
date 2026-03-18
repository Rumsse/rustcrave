using UnityEngine;

public abstract class EventAction : ScriptableObject
{
    public abstract void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState);
}
