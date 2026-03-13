using UnityEngine;
using UnityEngine.AI;

public enum UnitActivity
{
    Idle,
    Moving,
    Mining,
    Fighting,
    Interacting,
    Disabled
}

public class Unit : UnitBase
{
    public NavMeshAgent Agent => agent;
    public string Id => swarmUnitsData.id;

    [SerializeField] private UnitInventory inventory;
    [SerializeField] private EnergyManager energyManager;
    [SerializeField] private float minMiningTime;
    [SerializeField] private float minMoveSpeedMultiplier;
    [SerializeField] private float interactionStoppingDistance;
    [SerializeField] private float defaultStoppingDistance;
    [SerializeField, Range(0f, 1f)] private float lowEnergyThreshold;

    private float baseMoveSpeed;
    private float miningTimer;
    private float miningInterval;

    private SwarmUnitsData swarmUnitsData;
    private SwarmState swarmState;


    private IInteractable currentInteractable;
    private IMineable currentMineable;

    public void Initialize(SwarmUnitsData data, SwarmState state)
    {
        swarmUnitsData = data;
        swarmState = state;

        healthManager.InitializeHealth(swarmUnitsData.currentHP);
        healthManager.onDeath += () => swarmState.MarkDead(swarmUnitsData.id);
        healthManager.onHit += HandleDamageTaken;

        energyManager.InitializeEnergy(swarmUnitsData.currentEnergy);
        UnitRegistry.Register(this);
    }

    protected override void Awake()
    {
        base.Awake();
        baseMoveSpeed = agent.speed;
        miningTimer = 0f;
    }

    private void OnEnable()
    {
        TunnelEnd.OnTunnelEndReached += SyncDataToState;
        energyManager.onEnergyPercentChange += HandleMoveSpeedBasedOnEnergy;
        energyManager.onEnergyDepleted += HandleEnergyDepleted;
    }

    private void OnDisable()
    {
        TunnelEnd.OnTunnelEndReached -= SyncDataToState;
        energyManager.onEnergyPercentChange -= HandleMoveSpeedBasedOnEnergy;
        energyManager.onEnergyDepleted -= HandleEnergyDepleted;
        healthManager.onHit -= HandleDamageTaken;
    }

    private void OnDestroy()
    {
        UnitRegistry.Unregister(this);
    }

    protected override void Update()
    {
        base.Update();

        HandleEnergyDrain();

        if (isAttacking)
            return;

        if (currentMineable != null)
            HandleMining();

        if (currentInteractable != null)
            HandleInteraction();
    }

    private void HandleDamageTaken(int newCurrentHP)
    {
        if (swarmUnitsData != null)
        {
            swarmUnitsData.currentHP = newCurrentHP;
        }
    }

    private void HandleMoveSpeedBasedOnEnergy(float energyPercent)
    {
        if (energyPercent < lowEnergyThreshold)
        {
            float t = energyPercent / lowEnergyThreshold;
            agent.speed = Mathf.Lerp(minMoveSpeedMultiplier * baseMoveSpeed, baseMoveSpeed, t);
        }
        else
        {
            agent.speed = baseMoveSpeed;
        }
    }

    private void HandleInterruptCurrentAction()
    {
        currentMineable = null;
        currentInteractable = null;
        AttackTarget = null;
        miningTimer = 0f;
        animator.ResetTrigger("Mining");
    }

    private void HandleEnergyDepleted()
    {
        agent.isStopped = true;

        HandleInterruptCurrentAction();

        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Shutdown");

        this.enabled = false;
    }

    private void HandleEnergyDrain()
    {
        if (isAttacking)
        {
            energyManager.SetActionDrain();
        }
        else if (currentMineable != null || currentInteractable != null)
        {
            energyManager.SetActionDrain();
        }
        else if (agent.velocity.magnitude > 0.1f)
        {
            energyManager.SetMoveDrain();
        }
        else
        {
            energyManager.SetIdleDrain();
        }
    }

    private void HandleInteraction()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (currentInteractable is OrePickUp pickup)
            {
                if (inventory.InventorySO.AddItem(pickup.item, pickup.oreValueAmount))
                {
                    Destroy(pickup.gameObject);
                }
            }
            else
            {
                currentInteractable.Interact();
            }

            currentInteractable = null;
            animator.SetBool("IsWalking", false);
        }
        else
        {
            animator.SetBool("IsWalking", true);
        }
    }

    private void HandleMining()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (miningTimer == miningInterval)
            {
                animator.SetTrigger("Mining");
            }

            miningTimer -= Time.deltaTime;

            if (miningTimer <= 0f)
            {
                ItemSO item = currentMineable.Mine();

                if (item != null)
                {
                    if (currentMineable.IsDepleted())
                    {
                        currentMineable = null;
                    }
                    else
                    {
                        miningTimer = miningInterval;
                    }
                }
                else
                {
                    currentMineable = null;
                }
            }
            animator.SetBool("IsWalking", false);
        }
        else
        {
            animator.SetBool("IsWalking", true);
        }
    }

    public void MoveToInteract(IInteractable interactable, Vector3 position)
    {
        HandleInterruptCurrentAction();
        currentInteractable = interactable;

        agent.stoppingDistance = interactionStoppingDistance;
        agent.SetDestination(position);
    }

    public void MoveToMine(IMineable mineable, Vector3 position)
    {
        HandleInterruptCurrentAction();
        currentMineable = mineable;

        float rawMiningTime = currentMineable.GetDurability() - stats.MiningPower;
        miningInterval = Mathf.Max(minMiningTime, rawMiningTime);
        miningTimer = miningInterval;

        agent.stoppingDistance = interactionStoppingDistance;
        agent.SetDestination(position);
    }

    public override void MoveToAttack(UnitBase enemy, Vector3 position)
    {
        HandleInterruptCurrentAction();
        base.MoveToAttack(enemy, position);
    }

    public override void HandleMovement(Vector3 position)
    {
        HandleInterruptCurrentAction();

        agent.stoppingDistance = defaultStoppingDistance;
        base.HandleMovement(position);
    }

    public bool IsMining()
    {
        return currentMineable != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public float GetMiningProgress()
    {
        if (currentMineable == null || miningInterval <= 0f)
            return 0f;

        return 1f - (miningTimer / miningInterval);
    }

    public UnitActivity GetCurrentState()
    {
        if (isAttacking)
            return UnitActivity.Fighting;

        if (currentMineable != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                return UnitActivity.Mining;

            return UnitActivity.Moving;
        }

        if (agent.velocity.magnitude > 0.1f)
            return UnitActivity.Moving;

        return UnitActivity.Idle;
    }

    public void SyncDataToState()
    {
        if (swarmUnitsData != null && swarmUnitsData.isAlive)
        {
            swarmUnitsData.currentHP = healthManager.CurrentHP;
            swarmUnitsData.currentEnergy = energyManager.CurrentEnergy;
        }
    }
}