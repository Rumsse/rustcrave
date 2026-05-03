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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void ApplyPush(Vector3 pushDirection, float force)
    {
        if (isFalling) return;

        Vector3 moveDelta = pushDirection * force;
        Vector3 targetPosition = transform.position + moveDelta;

        NavMeshHit hit;
        if (agent.Raycast(targetPosition, out hit))
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