using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum UnitVillainState
{
    Patrolling,
    Fleeing
}

public class UnitController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private string targetTag;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float stopDistanceTolerance;
    [SerializeField] private float safeDistance;
    [SerializeField] private UnitBase _unit;

    private Transform targetTransform;
    private Transform currentFleeTarget;
    private int currentPointIndex;
    private bool isActive = true;
    private bool _isPausedForAttack;
    private UnitVillainState currentState = UnitVillainState.Patrolling;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private const float MovementThreshold = 0.01f;

    #region Unity Lifecycle

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || currentState == UnitVillainState.Fleeing)
            return;

        if (CheckForTargetTag(other.gameObject))
            targetTransform = other.transform;
    }

    private void Update()
    {
        UpdateAnimationState();

        if (_isPausedForAttack || _unit == null || !navMeshAgent.isActiveAndEnabled)
            return;

        if (targetTransform != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

            if (distanceToTarget >= safeDistance)
                ResumePatrolling();
            else
                HandleFleeing();
        }
        else if (currentState == UnitVillainState.Patrolling)
        {
            HandlePatrolling();
        }
    }

    #endregion

    #region Attack Interruption

    public void PauseForSpecialAttack()
    {
        _isPausedForAttack = true;

        if (!navMeshAgent.isActiveAndEnabled)
            return;

        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.isStopped = true;
    }

    public void ResumeFromSpecialAttack()
    {
        _isPausedForAttack = false;

        if (!navMeshAgent.isActiveAndEnabled)
            return;

        navMeshAgent.isStopped = false;
        MoveToCurrentWaypoint();
    }

    #endregion

    #region Logic

    public void ActivateUnit()
    {
        if (waypoints.Count == 0)
            return;

        isActive = true;
        currentPointIndex = 0;
        currentState = UnitVillainState.Patrolling;
        MoveToCurrentWaypoint();
    }

    public void DeactivateUnit()
    {
        isActive = false;
        navMeshAgent.ResetPath();
        targetTransform = null;
    }

    private void UpdateAnimationState()
    {
        if (!navMeshAgent.isActiveAndEnabled)
            return;

        bool isMoving = navMeshAgent.velocity.sqrMagnitude > MovementThreshold;
        animator.SetBool(IsWalkingHash, isMoving);
    }

    private void HandleFleeing()
    {
        if (currentState != UnitVillainState.Fleeing)
        {
            currentState = UnitVillainState.Fleeing;
            SetNewFleeTarget();
            return;
        }

        if (currentFleeTarget == null)
            return;

        float distancePlayerToFleeTarget = Vector3.Distance(targetTransform.position, currentFleeTarget.position);

        if (distancePlayerToFleeTarget < safeDistance)
            SetNewFleeTarget();
    }

    private void SetNewFleeTarget()
    {
        currentFleeTarget = GetFurthestWaypointFromTarget();

        if (navMeshAgent.isActiveAndEnabled)
            navMeshAgent.SetDestination(currentFleeTarget.position);
    }

    private void ResumePatrolling()
    {
        targetTransform = null;
        currentState = UnitVillainState.Patrolling;
        UpdatePatrolIndexToClosestWaypoint();
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
        if (navMeshAgent.isActiveAndEnabled)
            navMeshAgent.SetDestination(waypoints[currentPointIndex].position);
    }

    #endregion

    #region Helpers

    private void UpdatePatrolIndexToClosestWaypoint()
    {
        float minDistance = float.MaxValue;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position);

            if (distance >= minDistance)
                continue;

            minDistance = distance;
            currentPointIndex = i;
        }
    }

    private Transform GetFurthestWaypointFromTarget()
    {
        Transform bestPoint = waypoints[0];
        float maxDistance = float.MinValue;

        foreach (var point in waypoints)
        {
            float distance = Vector3.Distance(targetTransform.position, point.position);

            if (distance <= maxDistance)
                continue;

            maxDistance = distance;
            bestPoint = point;
        }

        return bestPoint;
    }

    private bool CheckForTargetTag(GameObject obj)
    {
        if (obj.CompareTag(targetTag))
            return true;

        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag(targetTag))
                return true;
        }

        return false;
    }

    #endregion
}