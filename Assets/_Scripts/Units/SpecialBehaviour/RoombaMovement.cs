using UnityEngine;
using UnityEngine.AI;

public class RoombaMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitAtPoint = 0f;

    private NavMeshAgent agent;
    private int currentIndex = 0;

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        currentIndex = 0;
        StartCoroutine(PatrolLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    System.Collections.IEnumerator PatrolLoop()
    {
        while (true)
        {
            agent.SetDestination(waypoints[currentIndex].position);

            yield return null;

            yield return new WaitUntil(() =>
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.1f);

            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }
}