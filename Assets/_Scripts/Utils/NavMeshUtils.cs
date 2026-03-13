using UnityEngine;
using UnityEngine.AI;

namespace Utils
{
    public static class NavMeshUtils
    {
        public static Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPos = new Vector3(randomCircle.x, 0, randomCircle.y) + center;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            return GetRandomNavMeshPoint(center, radius);
        }

        public static Vector3 GetNearestNavMeshPoint(Vector3 center)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(center, out hit, 5f, NavMesh.AllAreas);
            
            return NavMesh.SamplePosition(center, out hit, 5f, NavMesh.AllAreas)
                ? hit.position
                : center;
        }
    }
}
