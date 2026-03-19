using UnityEngine;
using UnityEngine.Events;

public class BossPhaseEvents : MonoBehaviour
{
    public UnityEvent<int> OnPhaseChangeEvent;

    private BossPhaseController _bossPhaseController;

    private void Awake()
    {
        _bossPhaseController = GetComponentInParent<BossPhaseController>();
        
        if(_bossPhaseController == null)
        {
            _bossPhaseController = GetComponentInChildren<BossPhaseController>();
        }
    }

    private void OnEnable()
    {
        _bossPhaseController.onPhaseChange += OnPhaseChange;
    }

    private void OnDisable()
    {
        _bossPhaseController.onPhaseChange -= OnPhaseChange;
    }

    private void OnPhaseChange(int phaseIndex)
    {
        OnPhaseChangeEvent.Invoke(phaseIndex);
    }
}
