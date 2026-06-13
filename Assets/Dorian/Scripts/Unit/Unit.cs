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
        miningTimer = 0f;

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

    private void OnDisable()
    {
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
        UnitRegistry.Unregister(this);

        if (!miningSoundInstance.isValid())
            return;

        miningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        miningSoundInstance.release();
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
        float currentPercent = stats.MaxEnergy > 0 ? (float)energyManager.CurrentEnergy / stats.MaxEnergy : 1f;
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
        AttackTarget = null;
        miningTimer = 0f;
        interactionTimer = 0f;
    }

    public void CancelActionAndPath()
    {
        if (currentMineable != null || currentInteractable != null || AttackTarget != null)
            HandleInterruptCurrentAction();

        if (!agent.hasPath)
            return;

        agent.ResetPath();
        animator.SetBool("IsWalking", false);
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
            animator.SetBool("IsWalking", false);
            return;
        }

        bool isCloseEnough = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;

        if (!isCloseEnough)
        {
            animator.SetBool("IsWalking", true);
            return;
        }

        animator.SetBool("IsWalking", false);

        if (interactionTimer >= currentInteractable.InteractionTime)
            currentInteractable.PlayEffect();

        if (interactionTimer > 0f)
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
            animator.SetBool("IsWalking", false);
            return;
        }

        bool isCloseEnough = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;

        if (!isCloseEnough)
        {
            animator.SetBool("IsWalking", true);
            return;
        }

        animator.SetBool("IsWalking", false);

        if (miningTimer >= miningInterval)
            StartMiningEffect();

        miningTimer -= Time.deltaTime;

        if (miningTimer > 0f)
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
    }

    public void MoveToMine(IMineable mineable, Vector3 position)
    {
        HandleInterruptCurrentAction();
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
        if (currentMineable == null || currentMineable.Equals(null) || miningInterval <= 0f)
            return 0f;

        return 1f - (miningTimer / miningInterval);
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
        if (animator != null && !string.IsNullOrEmpty(commandTriggerName))
            animator.SetTrigger(commandTriggerName);
    }

    #endregion

    #region Audio & Effects

    private void StartMiningEffect()
    {
        currentMineable.PlayEffect();

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

        miningSoundInstance.getPlaybackState(out PLAYBACK_STATE playbackState);
        if (playbackState == PLAYBACK_STATE.STOPPED)
            miningSoundInstance.start();
    }

    private void StopMiningEffect()
    {
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