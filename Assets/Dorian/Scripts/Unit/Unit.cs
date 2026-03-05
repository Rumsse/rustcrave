using UnityEngine;
using UnityEngine.AI;

public class Unit : UnitBase
{
    private IInteractable currentInteractable;
    private IMineable currentMineable;
    
    protected override void Update()
    {
        base.Update();

        if (isAttacking)
            return;

        if (currentMineable != null)
            HandleMining();

        if (currentInteractable != null)
            HandleInteraction();
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
            currentMineable.Mine();
            currentMineable = null;
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Mining");
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