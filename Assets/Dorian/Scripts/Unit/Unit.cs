using UnityEngine;
using UnityEngine.AI;

public class Unit : UnitBase
{
    [SerializeField] private UnitInventory inventory;
    [SerializeField] private EnergyManager energyManager;

    private float miningTimer;
    private float miningInterval;

    private IInteractable currentInteractable;
    private IMineable currentMineable;

    protected override void Awake()
    {
        base.Awake();

        miningInterval = stats.miningPower;
        miningTimer = 0f;
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
            currentInteractable.Interact();
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
            miningTimer -= Time.deltaTime;

            if (miningTimer <= 0f)
            {
                miningTimer = miningInterval;

                ItemSO item = currentMineable.Mine();

                if (item != null)
                {
                    inventory.InventorySO.AddItem(item,1);
                    animator.SetTrigger("Mining");
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
        AttackTarget = null;
        currentMineable = null;
        currentInteractable = interactable;
        agent.SetDestination(position);
    }

    public void MoveToMine(IMineable mineable, Vector3 position)
    {
        AttackTarget = null;
        currentInteractable = null;
        currentMineable = mineable;
        agent.SetDestination(position);
    }

    public override void MoveToAttack(UnitBase enemy, Vector3 position)
    {
        currentInteractable = null;
        currentMineable = null;

        base.MoveToAttack(enemy, position);
    }
    
    public override void HandleMovement(Vector3 position)
    {
        currentInteractable = null;
        currentMineable = null;
        AttackTarget = null;
        base.HandleMovement(position);
    }

}