using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public enum UnitActivity
{
    Idle,
    Moving,
    Mining,
    Fighting,
    Interacting,
    Disabled
}

public class Unit : UnitBase, ITrackableUnit
{
    #region Properties & Fields

    public static Unit MainCharacter { get; private set; }
    public static List<Unit> units = new();

    public string Id => swarmUnitsData.id;
    public bool IsMainCharacter => isMainCharacter;
    public bool IsEnergyDrainDoubled { get; set; }

    public UnitInventory Inventory => inventory;
    public Vector3 Position => transform.position;

    [SerializeField] private bool isMainCharacter;
    [SerializeField] private string commandTriggerName;
    [SerializeField] private UnitInventory inventory;
    [SerializeField] private UnitEquipment unitEquipment;
    [SerializeField] private EnergyManager energyManager;
    [SerializeField] private float minMiningTime;
    [SerializeField] private float minMoveSpeedMultiplier;
    [SerializeField] private float interactionStoppingDistance;
    [SerializeField] private float defaultStoppingDistance;
    [SerializeField, Range(0f, 1f)] private float lowEnergyThreshold;

    private const float MIN_VELOCITY_MAGNITUDE = 0.1f;
    private const float DISTANCE_TOLERANCE = 0.1f;
    private const float MAX_ENERGY_PERCENT = 1f;
    private const float EMPTY_PROGRESS = 0f;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsMiningHash = Animator.StringToHash("IsMining");
    private static readonly int ShutdownHash = Animator.StringToHash("Shutdown");
    private int commandTriggerHash;

    private float baseMoveSpeed;
    private float miningTimer;
    private float miningInterval;
    private float interactionTimer;

    private SwarmUnitsData swarmUnitsData;
    private SwarmState swarmState;

    private IInteractable currentInteractable;
    private IMineable currentMineable;

    private EventInstance miningSoundInstance;

    #endregion

    #region Initialization & Lifecycle

    public void Initialize(SwarmUnitsData data, SwarmState state)
    {
        swarmUnitsData = data;
        swarmState = state;

        healthManager.InitializeHealth(swarmUnitsData.currentHP);
        healthManager.onDeath += () => swarmState.MarkDead(swarmUnitsData.id);
        healthManager.onHit += HandleDamageTaken;

        energyManager.InitializeEnergy(swarmUnitsData.currentEnergy);

        if (unitEquipment != null)
            unitEquipment.Initialize(swarmUnitsData.assignedGadgets);

        UnitRegistry.Register(this);

        RefreshStats();
    }

    protected override void Awake()
    {
        base.Awake();
        baseMoveSpeed = agent.speed;
        miningTimer = EMPTY_PROGRESS;
        commandTriggerHash = Animator.StringToHash(commandTriggerName);

        if (isMainCharacter)
            MainCharacter = this;
    }

    private void OnEnable()
    {
        energyManager.onEnergyPercentChange += HandleMoveSpeedBasedOnEnergy;
        energyManager.onEnergyDepleted += HandleEnergyDepleted;
        healthManager.onHit += HandleDamageTaken;

        if (VisibilityManager.Instance != null)
            VisibilityManager.Instance.Register(this);

        if (!units.Contains(this))
            units.Add(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        energyManager.onEnergyPercentChange -= HandleMoveSpeedBasedOnEnergy;
        energyManager.onEnergyDepleted -= HandleEnergyDepleted;
        healthManager.onHit -= HandleDamageTaken;

        if (VisibilityManager.Instance != null)
            VisibilityManager.Instance.Unregister(this);

        if (units.Contains(this))
            units.Remove(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        UnitRegistry.Unregister(this);

        if (!miningSoundInstance.isValid())
            return;

        miningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        miningSoundInstance.release();
        miningSoundInstance.clearHandle();
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

    #endregion

    #region Core Logic

    public void RefreshStats()
    {
        if (stats == null)
            return;

        baseMoveSpeed = stats.MoveSpeed;
        float currentPercent = stats.MaxEnergy > EMPTY_PROGRESS ? (float)energyManager.CurrentEnergy / stats.MaxEnergy : MAX_ENERGY_PERCENT;
        HandleMoveSpeedBasedOnEnergy(currentPercent);
    }

    private void HandleDamageTaken(int newCurrentHP)
    {
        if (swarmUnitsData != null)
            swarmUnitsData.currentHP = newCurrentHP;
    }

    private void HandleMoveSpeedBasedOnEnergy(float energyPercent)
    {
        if (energyPercent >= lowEnergyThreshold)
        {
            agent.speed = baseMoveSpeed;
            return;
        }

        float t = energyPercent / lowEnergyThreshold;
        agent.speed = Mathf.Lerp(minMoveSpeedMultiplier * baseMoveSpeed, baseMoveSpeed, t);
    }

    private void HandleInterruptCurrentAction()
    {
        StopMiningEffect();

        if (currentInteractable != null)
            currentInteractable.StopEffect();

        currentMineable = null;
        currentInteractable = null;
        StopAttacking();
        miningTimer = EMPTY_PROGRESS;
        interactionTimer = EMPTY_PROGRESS;
    }

    public void CancelActionAndPath()
    {
        if (currentMineable != null || currentInteractable != null || AttackTarget != null)
            HandleInterruptCurrentAction();

        if (!agent.hasPath)
            return;

        agent.ResetPath();
        animator.SetBool(IsWalkingHash, false);
    }

    private void HandleEnergyDepleted()
    {
        agent.isStopped = true;

        HandleInterruptCurrentAction();

        animator.SetBool(IsWalkingHash, false);
        animator.SetTrigger(ShutdownHash);

        this.enabled = false;
    }

    private void HandleEnergyDrain()
    {
        if (isAttacking)
            energyManager.SetActionDrain();
        else if (currentMineable != null || currentInteractable != null)
            energyManager.SetActionDrain();
        else if (agent.velocity.magnitude > MIN_VELOCITY_MAGNITUDE)
        {
            if (IsEnergyDrainDoubled)
                energyManager.SetActionDrain();
            else
                energyManager.SetMoveDrain();
        }
        else
            energyManager.SetIdleDrain();
    }

    private void HandleInteraction()
    {
        if (currentInteractable == null || currentInteractable.Equals(null))
        {
            currentInteractable = null;
            animator.SetBool(IsWalkingHash, false);
            return;
        }

        bool isCloseEnough = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + DISTANCE_TOLERANCE;

        if (!isCloseEnough)
        {
            animator.SetBool(IsWalkingHash, true);
            return;
        }

        animator.SetBool(IsWalkingHash, false);

        if (interactionTimer >= currentInteractable.InteractionTime)
            currentInteractable.PlayEffect();

        if (interactionTimer > EMPTY_PROGRESS)
        {
            interactionTimer -= Time.deltaTime;
            return;
        }

        currentInteractable.StopEffect();

        if (currentInteractable is OrePickUp pickup)
        {
            if (inventory.InventorySO.AddItem(pickup.item, pickup.oreValueAmount))
            {
                pickup.Interact();
                Destroy(pickup.gameObject);
            }
            else
                inventory.TriggerInventoryFullAttempt();
        }
        else
            currentInteractable.Interact();

        currentInteractable = null;
    }

    private void HandleMining()
    {
        if (currentMineable == null || currentMineable.Equals(null) || currentMineable.IsDepleted())
        {
            StopMiningEffect();
            currentMineable = null;
            animator.SetBool(IsWalkingHash, false);
            return;
        }

        bool isCloseEnough = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + DISTANCE_TOLERANCE;

        if (!isCloseEnough)
        {
            animator.SetBool(IsWalkingHash, true);
            StopMiningEffect();
            return;
        }

        animator.SetBool(IsWalkingHash, false);

        if (miningTimer >= miningInterval)
            StartMiningEffect();

        miningTimer -= Time.deltaTime;

        if (miningTimer > EMPTY_PROGRESS)
            return;

        ItemSO item = currentMineable.Mine();

        if (item != null && !currentMineable.Equals(null) && !currentMineable.IsDepleted())
        {
            miningTimer = miningInterval;
            return;
        }

        StopMiningEffect();
        currentMineable = null;
    }

    #endregion

    #region Actions & Movement

    public void MoveToInteract(IInteractable interactable, Vector3 position)
    {
        HandleInterruptCurrentAction();
        SetState(null);

        currentInteractable = interactable;
        interactionTimer = currentInteractable.InteractionTime;

        agent.stoppingDistance = interactionStoppingDistance;

        Collider col = (interactable as MonoBehaviour)?.GetComponentInChildren<Collider>();
        if (col != null)
        {
            Vector3 dest = col.ClosestPoint(transform.position);
            dest.y = transform.position.y;
            agent.SetDestination(dest);
        }
        else
            agent.SetDestination(position);

        if (agent.isOnNavMesh)
            agent.isStopped = false;
    }

    public void MoveToMine(IMineable mineable, Vector3 position)
    {
        HandleInterruptCurrentAction();
        SetState(null);

        currentMineable = mineable;

        float rawMiningTime = currentMineable.GetDurability() - stats.MiningPower;
        miningInterval = Mathf.Max(minMiningTime, rawMiningTime);
        miningTimer = miningInterval;

        agent.stoppingDistance = interactionStoppingDistance;

        Collider col = (mineable as MonoBehaviour)?.GetComponentInChildren<Collider>();
        if (col != null)
        {
            Vector3 dest = col.ClosestPoint(transform.position);
            dest.y = transform.position.y;
            agent.SetDestination(dest);
        }
        else
            agent.SetDestination(position);

        if (agent.isOnNavMesh)
            agent.isStopped = false;
    }

    public override void MoveToAttack(UnitBase enemy, Vector3 position)
    {
        if (AttackTarget == enemy)
            return;

        HandleInterruptCurrentAction();
        base.MoveToAttack(enemy, position);
    }

    public override void HandleMovement(Vector3 position)
    {
        if (MCFormController.Instance != null)
            MCFormController.Instance.CancelReturn();

        HandleInterruptCurrentAction();

        agent.stoppingDistance = defaultStoppingDistance;
        base.HandleMovement(position);
    }

    #endregion

    #region Utility

    public bool IsMining()
    {
        return currentMineable != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public float GetMiningProgress()
    {
        if (currentMineable == null || currentMineable.Equals(null) || miningInterval <= EMPTY_PROGRESS)
            return EMPTY_PROGRESS;

        return MAX_ENERGY_PERCENT - (miningTimer / miningInterval);
    }

    public UnitActivity GetCurrentState()
    {
        if (isAttacking)
            return UnitActivity.Fighting;

        if (currentMineable != null && !currentMineable.Equals(null))
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                return UnitActivity.Mining;

            return UnitActivity.Moving;
        }

        if (agent.velocity.magnitude > MIN_VELOCITY_MAGNITUDE)
            return UnitActivity.Moving;

        return UnitActivity.Idle;
    }

    public static Unit GetRandomUnit() => units[Random.Range(0, units.Count)];

    public void SyncDataToState()
    {
        if (swarmUnitsData == null || !swarmUnitsData.isAlive)
            return;

        swarmUnitsData.currentHP = healthManager.CurrentHP;
        swarmUnitsData.currentEnergy = energyManager.CurrentEnergy;

        inventory.InventorySO.TransferTo(swarmState.GlobalInventory);
    }

    public void SetAnimator(Animator newAnimator) => animator = newAnimator;

    public void PlayCommandAnimation()
    {
        if (animator != null && commandTriggerHash != 0)
            animator.SetTrigger(commandTriggerHash);
    }

    #endregion

    #region Audio & Effects

    private void StartMiningEffect()
    {
        currentMineable.PlayEffect();

        if (animator != null)
            animator.SetBool(IsMiningHash, true);

        if (!miningSoundInstance.isValid())
        {
            EventReference soundToPlay = stats.Sounds.mineSound;
            OreSO targetOre = currentMineable.GetOreData();

            if (targetOre != null && stats.Sounds.oreMiningSounds != null)
            {
                foreach (OreMiningSound oreSound in stats.Sounds.oreMiningSounds)
                {
                    if (oreSound.ore != targetOre)
                        continue;

                    soundToPlay = oreSound.sound;
                    break;
                }
            }

            miningSoundInstance = RuntimeManager.CreateInstance(soundToPlay);
        }

        miningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        miningSoundInstance.start();
    }

    private void StopMiningEffect()
    {
        if (animator != null)
            animator.SetBool(IsMiningHash, false);

        if (currentMineable != null && !currentMineable.Equals(null))
            currentMineable.StopEffect();

        if (!miningSoundInstance.isValid())
            return;

        miningSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        miningSoundInstance.release();
        miningSoundInstance.clearHandle();
    }

    public void PlaySelectSound() => AudioManager.PlayOneShot(stats.Sounds.selectSound);

    public void PlayCommandSound() => AudioManager.PlayOneShot(stats.Sounds.commandSound);

    #endregion
}