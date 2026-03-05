using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public abstract class UnitBase : MonoBehaviour
{
    #region Properties

    public UnitSO Stats => stats;
    public Animator Animator => animator;
    public HealthManager HealthManager => healthManager;

    public UnitBase AttackTarget
    {
        get => attackTarget;
        set
        {
            Debug.Log($"{gameObject.name} {value}");
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
    [SerializeField] protected UnitSO stats;
    [SerializeField] protected Animator animator;
    [SerializeField] protected HealthManager healthManager;

    #endregion
    
    #region Private Fields
    
    protected NavMeshAgent agent;
    private UnitBase attackTarget;

    protected AttackBase currentAttack;
    protected float lastAttackTime;

    protected bool isAttacking;
    
    #endregion
    
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = stats.moveSpeed;
        agent.stoppingDistance = stoppingDistance;
    }
    
    protected virtual void Update()
    {
        UpdateAnimation();
        
        if (AttackTarget && currentAttack)
        {
            HandleAttacking();
            isAttacking = true;
            return;
        }
        
        isAttacking = false;
    }
    
    private void UpdateAnimation()
    {
        bool isWalking = agent.hasPath && agent.remainingDistance > agent.stoppingDistance && agent.velocity.sqrMagnitude > 0.01f;
    
        animator.SetBool("IsWalking", isWalking);
    }
    
    public virtual void HandleMovement(Vector3 position)
    {
        agent.SetDestination(position);
    }

    private void HandleAttacking()
    {
        if (!agent.pathPending && Vector3.Distance(transform.position, AttackTarget.transform.position) <= currentAttack.attackRange)
        {
            TryAttack();
            agent.SetDestination(transform.position);
        }
        else
        {
            agent.SetDestination(AttackTarget.transform.position);
            animator.SetBool("IsWalking", true);
        }
    }
    
    public virtual void MoveToAttack(UnitBase enemy, Vector3 position)
    {
        AttackTarget = enemy;
        agent.SetDestination(enemy.transform.position);

        attackTarget.HealthManager.onDeath += StopAttacking;
    }


    protected virtual void TryAttack()
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
        currentAttack = stats.possibleAttacks[Random.Range(0, stats.possibleAttacks.Count)];
    }

    protected virtual bool CanAttack()
    {
        if (Time.time - lastAttackTime < 1 / stats.attacksPerSecond)
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
}
