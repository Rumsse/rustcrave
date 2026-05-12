using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : UnitBase
{
    [SerializeField] protected Transform guardPoint;
    [SerializeField] protected bool specialUnit;

    public ItemSO StolenItem { get; private set; } // saves stolen item to drop later

    protected List<Unit> playerUnits = new();
    
    private Dictionary<Unit, Action> deathCallbacks = new();
    protected bool _available = true;

    #region Unity Lifecycle


    protected override void OnDestroy()
    {
        base.OnDestroy();

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

        if (!_available || isAttacking || Stats.PossibleAttacks.Count == 0 || !agent.enabled)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetBool("IsWalking", false);
            return;
        }

        animator.SetBool("IsWalking", true);

        if (Vector3.Distance(agent.destination, guardPoint.position) > 0.1f)
            agent.SetDestination(guardPoint.position);
    }

    #endregion

    #region Registering Units

    public void UnitEnter(Unit unit)
    {
        if (!_available) return;

        if (playerUnits.Contains(unit))
            return;

        playerUnits.Add(unit);

        if (!AttackTarget)
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
        if (!_available) return;

        if (!playerUnits.Remove(unit))
            return;

        if (deathCallbacks.TryGetValue(unit, out Action callback))
        {
            unit.HealthManager.onDeath -= callback;
            deathCallbacks.Remove(unit);
        }

        if (AttackTarget == unit)
        {
            Unit newTarget = GetClosestUnit();

            if (newTarget != null)
                AttackTarget = newTarget;
            else
                StopAttacking();
        }
    }

    private void RemoveOnDeath(Unit unit) => RemoveUnit(unit);

    #region Special Enemies Behavior 
    protected virtual void HandleSpecialReaction() { }
    public override bool SpecialReactionForUnit() => specialUnit;
    #endregion

    #region Helper

    private Unit GetClosestUnit()
    {
        if (playerUnits.Count == 0)
            return null;

        Unit closest = playerUnits[0];
        float closestDist = Vector3.Distance(transform.position, closest.transform.position);

        for (int i = 1; i < playerUnits.Count; i++)
        {
            if (playerUnits[i] == null)
                continue;

            float dist = Vector3.Distance(transform.position, playerUnits[i].transform.position);

            if (dist >= closestDist)
                continue;

            closestDist = dist;
            closest = playerUnits[i];
        }

        return closest;
    }

    private void NullCleanup() => playerUnits.RemoveAll(unit => !unit);

    #endregion

    #region Stolen Item

    //managing stolen item 
    public virtual void StealItem(ItemSO item)
    {
        StolenItem = item;
        HandleSpecialReaction();
    }

    public virtual void ClearStolenItem()
    {
        StolenItem = null;
    }

    #endregion
}
