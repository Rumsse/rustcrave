using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public abstract class UnitBase : MonoBehaviour
{
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
            
            if(value)
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
    [SerializeField] protected Transform modelMidPoint; // used for projectiles to aim at model chest / mid point instead of pivot
    
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

    public virtual void TryAttack()
    {
        if (CanAttack())
        {
            currentAttack.Execute(AttackTarget, this);
            RollAttack();
            lastAttackTime = Time.time;
        }
    }

    protected void RollAttack()
    {
        currentAttack = stats.PossibleAttacks[Random.Range(0, stats.PossibleAttacks.Count)];
    }

    protected virtual bool CanAttack()
    {
        if (Time.time - lastAttackTime < 1 / stats.AttacksPerSecond)
            return false;
        
        // can add stuff like HasStun() etc.
        
        return true;
    }

    protected void StopAttacking()
    {
        if(AttackTarget)
            AttackTarget.HealthManager.onDeath -= StopAttacking;
        
        AttackTarget = null;
        currentAttack = null;
    }
    
    public void SetState(UnitState newState)
    {
        state?.ExitState();
        state = newState;
        state?.EnterState();
    }
    
    public void ChangeStats(UnitSO newStats) => stats.ChangeStats(newStats);
}
