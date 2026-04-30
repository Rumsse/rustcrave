using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;
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
    public NavMeshAgent Agent => agent;
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

            if (value && !currentAttack)
                RollAttack();
        }
    }

    public bool IsPerformingSpecial
    {
        get => _isPerformingSpecial;
        set
        {
            _isPerformingSpecial = value;

            if (!value)
            {
                lastAttackTime = Time.time;
                onSpecialStop?.Invoke();
            }
            else
                onSpecialStart?.Invoke();
        }
    }

    #endregion

    #region Inspector Fields

    [SerializeField] protected float rotateSpeed;
    [SerializeField] protected float stoppingDistance;
    [SerializeField] protected bool loopThroughAttacks;

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

    private int currentAttackIndex;
    private bool _isPerformingSpecial;

    private EventInstance attackSoundInstance;

    #endregion

    #region Unity Lifecycle

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

    protected virtual void OnDestroy()
    {
        if (attackSoundInstance.isValid())
        {
            attackSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            attackSoundInstance.release();
        }
    }

    #endregion

    #region State Management

    public void SetState(UnitState newState)
    {
        state?.ExitState();
        state = newState;
        state?.EnterState();

        onStateChange?.Invoke(state);
    }

    public virtual void HandleMovement(Vector3 position)
    {
        StopAttacking();
        SetState(new WalkingState(this, position));
    }

    public virtual void MoveToAttack(UnitBase enemy, Vector3 position)
    {
        AttackTarget = enemy;
        SetState(new FightState(this));

        enemy.HealthManager.onDeath += StopAttacking;
    }

    public virtual bool SpecialReactionForUnit() => false;

    #endregion

    #region Attacking

    public virtual void TryAttack()
    {
        if (CanAttack())
        {
            onAttack?.Invoke();

            if (TryGetComponent<MantisPassiveAbility>(out var mantisPassive))
            {
                mantisPassive.ExecuteComboAttack(AttackTarget, currentAttack);
                PlayAttackSound();
            }
            else
            {
                PlayAttackSound();
                currentAttack.Execute(AttackTarget, this);
            }

            RollAttack();
            lastAttackTime = Time.time;
        }
    }
    #endregion

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

        StopAttackSound();
    }

    #endregion

    #region Audio Management

    protected void PlayAttackSound()
    {
        if (!attackSoundInstance.isValid())
        {
            attackSoundInstance = RuntimeManager.CreateInstance(stats.Sounds.attackSound);
            RuntimeManager.AttachInstanceToGameObject(attackSoundInstance, gameObject);
        }

        attackSoundInstance.getPlaybackState(out PLAYBACK_STATE playbackState);
        if (playbackState == PLAYBACK_STATE.STOPPED)
        {
            attackSoundInstance.start();
        }
    }

    protected virtual void StopAttackSound()
    {
        if (attackSoundInstance.isValid())
        {
            attackSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
    #endregion

    #region Phase Handling

    public void ChangeStats(UnitSO newStats)
    {
        stats.ChangeStats(newStats);
        RollAttackPhaseChange();
    }

    #endregion
}