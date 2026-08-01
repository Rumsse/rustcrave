using System;

[Serializable]
public abstract class EventAction
{
    public abstract void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState);
}