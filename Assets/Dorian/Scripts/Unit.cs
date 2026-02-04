using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private UnitSO unit;
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private Vector3 targetPosition;
    private IInteractable currentInteractable;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = unit.moveSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        UpdateAnimation();

        if (currentInteractable != null)
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
    }

    private void UpdateAnimation()
    {
        bool isWalking = agent.hasPath && agent.remainingDistance > agent.stoppingDistance && agent.velocity.sqrMagnitude > 0.01f;

        animator.SetBool("IsWalking", isWalking);
    }

    public void HandleMovement(Vector3 position)
    {
        currentInteractable = null;
        agent.SetDestination(position);
    }

    public void MoveToInteract(IInteractable interactable, Vector3 position)
    {
        currentInteractable = interactable;
        agent.SetDestination(position);
    }

}