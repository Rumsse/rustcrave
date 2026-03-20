using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class BossCoreP2Spawner : MonoBehaviour
{
    [SerializeField] private Transform _unitTransform;
    [SerializeField] private Collider _col;
    [SerializeField] private Transform _startPos;
    [SerializeField] private float _jumpHeight = 1f;
    [SerializeField] private List<Transform> _possibleEndPositions = new();
    [SerializeField] private TweenSettings _positionSettings;
    [SerializeField] private int _phaseIndex;
    
    [Header("Unit")]
    [SerializeField] private UnitSO _unit;
    
    public void Execute(int phaseIndex)
    {
        if (phaseIndex != _phaseIndex)
            return;
        
        Vector3 targetPos =  _possibleEndPositions[Random.Range(0, _possibleEndPositions.Count)].position;

        var unit = _unitTransform;
        _col.enabled = false;
        unit.gameObject.SetActive(true);
        
        var agent = unit.GetComponentInChildren<NavMeshAgent>();
        agent.enabled = false;
        
        unit.position = _startPos.position;

        Tween.PositionX(unit, new TweenSettings<float>(targetPos.x, _positionSettings));
        Tween.PositionZ(unit, new TweenSettings<float>(targetPos.z, _positionSettings));

        TweenSettings yPosSettings = _positionSettings;
        yPosSettings.duration *= .5f;
        Tween.PositionY(unit, new TweenSettings<float>(_jumpHeight, yPosSettings))
            .Chain(Tween.PositionY(unit, new TweenSettings<float>(targetPos.y, yPosSettings)))
            .OnComplete(() => {
                agent.enabled = true;
                _col.enabled = true;
            });
    }
}
