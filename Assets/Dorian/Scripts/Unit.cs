using UnityEngine;
using UnityEngine.AI;

public class Unit : UnitBase
{
    private IInteractable currentInteractable;
    
    protected override void Update()
    {
        base.Update();

        if (isAttacking)
            return;
        
        if (currentInteractable != null)
            HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentInteractable.Interact();
            currentInteractable = null;
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }
    
    public void MoveToInteract(IInteractable interactable, Vector3 position)
    {
        AttackTarget = null;
        currentInteractable = interactable;
        agent.SetDestination(position);
    }

    public override void MoveToAttack(UnitBase enemy, Vector3 position)
    {
        currentInteractable = null;
        
        base.MoveToAttack(enemy, position);
    }
    
    public override void HandleMovement(Vector3 position)
    {
        currentInteractable = null;
        AttackTarget = null;
        base.HandleMovement(position);
    }
}