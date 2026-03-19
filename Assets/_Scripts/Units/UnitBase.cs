using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public abstract class UnitBase : MonoBehaviour
{
    #region Events

    public event Action onAttack;
    public event Action onSpecialStart;
    public event Action onSpecialStop;
    public event Action<UnitState> onStateChange;

    #endregion
    
    #region Properties

    public StatsManager Stats => stats;
    public Animator Animator => animator;
    public HealthManager HealthManager => healthManager;
    public Transform ProjectileSpawnT => projectileSpawnT;
    public Transform ModelMidPoint => modelMidPoint;
    public AttackBase CurrentAttack => currentAttack;
    public UnitBase AttackTarget
    {
        get => attackTarget;
        set
        {
            attackTarget = value;
            isAttacking = value;

            if (value)
                RollAttack();
        }
    }

    #endregion

    #region Inspector Fields

    [SerializeField] protected float rotateSpeed;
    [SerializeField] protected float stoppingDistance;

    [Header("References")]
    [SerializeField] protected StatsManager stats;
    [SerializeField] protected Animator animator;
    [SerializeField] protected HealthManager healthManager;
    [SerializeField] protected Transform projectileSpawnT;
    [SerializeField] protected Transform modelMidPoint;

    #endregion

    #region Private Fields

    protected NavMeshAgent agent;
    private UnitBase attackTarget;

    protected AttackBase currentAttack;
    protected float lastAttackTime;

    protected bool isAttacking;

    private UnitState state;

    #endregion

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = stats.MoveSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    private void Start()
    {
        SetState(new IdleState(this));
    }

    protected virtual void Update()
    {
        state?.Tick();
    }

    public virtual void HandleMovement(Vector3 position)
    {
        AttackTarget = null;
        SetState(new WalkingState(this, position));
    }

    public virtual void MoveToAttack(UnitBase enemy, Vector3 position)
    {
        AttackTarget = enemy;
        SetState(new FightState(this));

        enemy.HealthManager.onDeath += StopAttacking;
    }

    #endregion

    #region Attacking

    public virtual void TryAttack()
    {
        if (CanAttack())
        {
            if (TryGetComponent<MantisPassiveAbility>(out var mantisPassive))
            {
                mantisPassive.ExecuteComboAttack(AttackTarget, currentAttack);
            }
            else
            {
                currentAttack.Execute(AttackTarget, this);
            }

            RollAttack();
            lastAttackTime = Time.time;
        }
    }

    #region Attack Chosing

    protected void RollAttack()
    {
        if (!loopThroughAttacks) RollAttackRandom();
        else RollAttackIterative();
    }

    private void RollAttackRandom() => currentAttack = stats.PossibleAttacks[Random.Range(0, stats.PossibleAttacks.Count)];

    private void RollAttackIterative()
    {
        if (currentAttackIndex >= stats.PossibleAttacks.Count)
            currentAttackIndex = 0;
        
        currentAttack = stats.PossibleAttacks[currentAttackIndex];
        
        currentAttackIndex++;
    }

    private void RollAttackPhaseChange()
    {
        int attackIndex = currentAttackIndex - 1;
        
        if (attackIndex >= stats.PossibleAttacks.Count || attackIndex < 0)
            attackIndex = 0;
        
        currentAttack = stats.PossibleAttacks[attackIndex];
    }
    
    #endregion
    
    
    protected virtual bool CanAttack()
    {
        if (Time.time - lastAttackTime < 1 / stats.AttacksPerSecond)
            return false;

        return true;
    }

    protected void StopAttacking()
    {
        if (AttackTarget)
            AttackTarget.HealthManager.onDeath -= StopAttacking;

        AttackTarget = null;
        currentAttack = null;
    }

    public void SetState(UnitState newState)
    {
        stats.ChangeStats(newStats);
        RollAttackPhaseChange();
    }

    public void ChangeStats(UnitSO newStats) => stats.ChangeStats(newStats);
}