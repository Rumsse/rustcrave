using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private string targetTag;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float stopDistanceTolerance;
    [SerializeField] private float safeDistance;

    private Transform targetTransform;
    private int currentPointIndex;
    private bool isActive;
    private bool isFleeing;
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    public void ActivateUnit()
    {
        if (waypoints.Count == 0) return;

        isActive = true;
        currentPointIndex = 0;
        animator.SetBool(IsWalkingHash, true);
        MoveToCurrentWaypoint();
    }

    public void DeactivateUnit()
    {
        isActive = false;
        navMeshAgent.ResetPath();
        animator.SetBool(IsWalkingHash, false);
        targetTransform = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || isFleeing) return;

        if (CheckForTargetTag(other.gameObject))
        {
            targetTransform = other.transform;
        }
    }

    private void Update()
    {
        //if (!isActive) return;

        if (targetTransform != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

            if (distanceToTarget >= safeDistance)
            {
                targetTransform = null;
                ResumePatrolling();
            }
            else
            {
                HandleFleeing();
            }
        }
        else if (!isFleeing)
        {
            HandlePatrolling();
        }
    }

    private void HandleFleeing()
    {
        isFleeing = true;
        Transform bestPoint = GetFurthestWaypointFromTarget();
        navMeshAgent.SetDestination(bestPoint.position);
        animator.SetBool(IsWalkingHash, true);
    }

    private void ResumePatrolling()
    {
        isFleeing = false;
        MoveToCurrentWaypoint();
    }

    private void HandlePatrolling()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= stopDistanceTolerance)
        {
            currentPointIndex = (currentPointIndex + 1) % waypoints.Count;
            MoveToCurrentWaypoint();
        }
    }

    private void MoveToCurrentWaypoint()
    {
        navMeshAgent.SetDestination(waypoints[currentPointIndex].position);
    }

    private Transform GetFurthestWaypointFromTarget()
    {
        Transform bestPoint = waypoints[0];
        float maxDistance = float.MinValue;

        foreach (var point in waypoints)
        {
            float distance = Vector3.Distance(targetTransform.position, point.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    private bool CheckForTargetTag(GameObject obj)
    {
        if (obj.CompareTag(targetTag)) return true;

        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(targetTag)) return true;
        }

        return false;
    }
}