using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitRunToDoors : MonoBehaviour
{
    [SerializeField] private UnitBase _unit;
    [SerializeField] private List<Transform> _doorTransforms;

    private bool _isRunning;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Units"))
            return;

        if (_isRunning)
            return;
        
        _isRunning = true;
        
        var unit = other.gameObject.GetComponentInChildren<Unit>();
        if(unit)
            _unit.HandleMovement(SelectBestPoint(other.transform).position);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Units"))
            return;

        _isRunning = false;
        
        _unit.HandleMovement(_unit.transform.position);
    }

    public Transform SelectBestPoint(Transform player)
    {
        if (_doorTransforms.Count < 2) return _doorTransforms[0];

        int currentIndex = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < _doorTransforms.Count; i++)
        {
            float d = Vector3.Distance(_unit.transform.position, _doorTransforms[i].position);
            if (d < minDist)
            {
                minDist = d;
                currentIndex = i;
            }
        }

        Vector3 playerPushDir = (_unit.transform.position - player.position).normalized;
    
        int nextIdx = (currentIndex + 1) % _doorTransforms.Count;
        int prevIdx = (currentIndex - 1 + _doorTransforms.Count) % _doorTransforms.Count;

        Vector3 dirToNext = (_doorTransforms[nextIdx].position - _unit.transform.position).normalized;
        Vector3 dirToPrev = (_doorTransforms[prevIdx].position - _unit.transform.position).normalized;

        float dotNext = Vector3.Dot(playerPushDir, dirToNext);
        float dotPrev = Vector3.Dot(playerPushDir, dirToPrev);

        return dotNext > dotPrev ? _doorTransforms[nextIdx] : _doorTransforms[prevIdx];
    }
    
    private float CalculatePathLength(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path))
        {
            if (path.status != NavMeshPathStatus.PathComplete) return -1f;

            float distance = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            return distance;
        }
        return -1f;
    }
}
