using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class PushableNavMeshEntity : MonoBehaviour, IPushable
{
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private float groundCheckDepth = 2f;
    [SerializeField] private float fallPushForce = 5f;
    [SerializeField] private LayerMask groundLayer = ~0;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private bool isFalling;
    private MCFormController formController;

    public bool CanBePushed
    {
        get
        {
            if (isFalling)
                return false;

            if (formController != null && formController.IsTransitioning)
                return false;

            return true;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        formController = GetComponent<MCFormController>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void ApplyPush(Vector3 pushDirection, float force)
    {
        if (!CanBePushed)
            return;

        Vector3 moveDelta = pushDirection * force;
        Vector3 targetPosition = transform.position + moveDelta;

        if (agent.Raycast(targetPosition, out NavMeshHit hit))
        {
            Vector3 checkPosition = hit.position + (pushDirection * edgeCheckDistance);
            Vector3 rayStart = checkPosition + Vector3.up;

            if (!Physics.Raycast(rayStart, Vector3.down, groundCheckDepth, groundLayer))
            {
                TriggerFall(pushDirection);
                return;
            }
        }

        agent.Move(moveDelta);
    }

    private void TriggerFall(Vector3 pushDirection)
    {
        isFalling = true;
        agent.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        transform.position += pushDirection * edgeCheckDistance;
        rb.AddForce(pushDirection * fallPushForce, ForceMode.VelocityChange);
    }
}