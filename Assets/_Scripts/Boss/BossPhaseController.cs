using System.Collections.Generic;
using UnityEngine;

public class BossPhaseController : MonoBehaviour
{
    [SerializeField] private HealthManager _healthManager;
    [SerializeField] private UnitBase _bossUnit;
    [SerializeField] private List<BossPhase> _phases;

    private int _nextPhaseIndex = 0;

    private void OnEnable() => _healthManager.onHealthPercentChange += OnHealthPercentChange;
    private void OnDisable() => _healthManager.onHealthPercentChange -= OnHealthPercentChange;

    private void OnHealthPercentChange(float healthPercent)
    {
        if (_nextPhaseIndex < _phases.Count && healthPercent <= _phases[_nextPhaseIndex].healthPercent)
        {
            ApplyPhase(_phases[_nextPhaseIndex]);
            _nextPhaseIndex++;
        }
    }

    private void ApplyPhase(BossPhase phase)
    {
        if(string.IsNullOrEmpty(phase.animationTrigger))
            _bossUnit.Animator.SetTrigger(phase.animationTrigger);

        _bossUnit.ChangeStats(phase.newStats);
    }
}
