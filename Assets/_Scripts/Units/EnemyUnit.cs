using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : UnitBase
{
    [SerializeField] protected Transform guardPoint;
    [SerializeField] protected bool specialUnit;
    [SerializeField] protected string specialAnimationName;

    public ItemSO StolenItem { get; private set; } // saves stolen item to drop later


    protected List<Unit> playerUnits = new();
    protected bool _available = true;
    protected bool hasExternalController;

    private Dictionary<Unit, Action> deathCallbacks = new();

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private const float DISTANCE_TOLERANCE = 0.1f;

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        hasExternalController = GetComponent<UnitController>() != null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ForceStopAttackSound();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var kvp in deathCallbacks)
        {
            if (kvp.Key != null && kvp.Key.HealthManager != null)
                kvp.Key.HealthManager.onDeath -= kvp.Value;
        }

        deathCallbacks.Clear();
    }

    protected override void Update()
    {
        base.Update();

        if (!_available || isAttacking || IsPerformingSpecial || Stats.PossibleAttacks.Count == 0 || !agent.enabled)
            return;

        if (hasExternalController)
            return;

        if (guardPoint != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                animator.SetBool(IsWalkingHash, false);
                HandleSpecialEffects();
                return;
            }

            animator.SetBool(IsWalkingHash, true);

            if ((agent.destination - guardPoint.position).sqrMagnitude > DISTANCE_TOLERANCE * DISTANCE_TOLERANCE)
                agent.SetDestination(guardPoint.position);
        }
    }

    #endregion

    #region Registering Units

    public void UnitEnter(Unit unit)
    {
        if (!_available || playerUnits.Contains(unit))
        {
            Debug.Log("Unit is unavailable.");
            return;
        }

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

    #region Helper

    private void RemoveUnit(Unit unit)
    {
        if (!playerUnits.Remove(unit))
        {
            Debug.Log("Unit is unavailable.");
            return;
        }

        if (deathCallbacks.TryGetValue(unit, out Action callback))
        {
            if (unit != null && unit.HealthManager != null)
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

    private Unit GetClosestUnit()
    {
        NullCleanup();

        if (playerUnits.Count == 0)
            return null;

        Unit closest = null;
        float closestDistSqr = float.MaxValue;

        foreach (var playerUnit in playerUnits)
        {
            if (playerUnit == null)
                continue;

            float distSqr = (transform.position - playerUnit.transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = playerUnit;
            }
        }

        return closest;
    }

    private void NullCleanup() => playerUnits.RemoveAll(unit => !unit);

    #endregion

    #region Special Enemy Behavior

    protected virtual void HandleSpecialReaction() { }
    public virtual void HandleSpecialEffects() { }
    public override bool SpecialReactionForUnits() => specialUnit;
    public virtual bool FarDetectEnabled() => true;

    #endregion

    #region Stolen Item

    //managing stolen item 
    public virtual void StealItem(ItemSO item)
    {
        StolenItem = item;
    }

    public virtual void ClearStolenItem()
    {
        StolenItem = null;
    }

    #endregion
}