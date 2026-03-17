using System;
using System.Collections.Generic;

public static class UnitRegistry
{
    public static event Action<Unit> OnUnitRegistered;

    private static Dictionary<string, Unit> units = new();


    public static void Register(Unit unit)
    {
        if (!units.ContainsKey(unit.Id))
            units.Add(unit.Id, unit);

        OnUnitRegistered?.Invoke(unit);
    }

    public static void Unregister(Unit unit)
    {
        if (units.ContainsKey(unit.Id))
            units.Remove(unit.Id);
    }

    public static Unit Get(string id)
    {
        units.TryGetValue(id, out Unit unit);
        return unit;
    }
}