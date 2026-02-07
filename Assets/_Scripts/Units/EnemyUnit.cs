using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class EnemyUnit : UnitBase
{
    [SerializeField] private Transform guardPoint;
    
    private List<Unit> playerUnits = new();
    
    private Dictionary<Unit, Action> deathCallbacks = new();
    
    #region Unity Lifecycle

    private void OnDestroy()
    {
        foreach (var kvp in deathCallbacks)
        {
            if (kvp.Key)
                kvp.Key.HealthManager.onDeath -= kvp.Value;
        }
        deathCallbacks.Clear();
    }

    protected override void Update()
    {
        base.Update();
        
        if(!isAttacking)
            agent.SetDestination(guardPoint.position);
    }

    #endregion

    #region Registering Units

    public void UnitEnter(Unit unit)
    {
        if (playerUnits.Contains(unit))
            return;
        
        playerUnits.Add(unit);
        
        if(!AttackTarget)
            AttackTarget = unit;

        Action callback = () => RemoveOnDeath(unit);
        deathCallbacks[unit] = callback;
        unit.HealthManager.onDeath += callback;
    }

    public void UnitLeave(Unit unit)
    {
        NullCleanup();
        RemoveUnit(unit);
    }

    #endregion
    
    private void RemoveUnit(Unit unit)
    {
        if (!playerUnits.Remove(unit))
            return;
        
        if (deathCallbacks.TryGetValue(unit, out Action callback))
        {
            unit.HealthManager.onDeath -= callback;
            deathCallbacks.Remove(unit);
        }
        
        if (AttackTarget == unit)
            AttackTarget = GetClosestUnit();
    }

    private void RemoveOnDeath(Unit unit)
    {
        RemoveUnit(unit);
    }

    #region Helper

    private Unit GetClosestUnit()
    {
        if (playerUnits.Count == 0) return null;
        
        Unit closest = playerUnits[0];
        float closestDist = Vector3.Distance(transform.position, closest.transform.position);
        
        for (int i = 1; i < playerUnits.Count; i++)
        {
            if (playerUnits[i] == null) continue;
            
            float dist = Vector3.Distance(transform.position, playerUnits[i].transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = playerUnits[i];
            }
        }
        
        return closest;
    }
    
    private void NullCleanup()
    {
        playerUnits.RemoveAll(unit => !unit);
    }

    #endregion
}
