using UnityEngine;
using UnityEngine.Events;
using System;

public class HealthManagerEvents : MonoBehaviour
{
    public UnityEvent<float> onHealthPercentChange;
    public UnityEvent<int> onHit;
    public UnityEvent onDeath;

    public static event Action OnAnyDeath;

    private HealthManager _healthManager;

    private void Awake()
    {
        _healthManager = GetComponent<HealthManager>();
    }

    private void OnEnable()
    {
        _healthManager.onDeath += OnDeath;
        _healthManager.onHit += OnHit;
        _healthManager.onHealthPercentChange += OnHealthPercentChange;
    }

    private void OnDisable()
    {
        _healthManager.onDeath -= OnDeath;
        _healthManager.onHit -= OnHit;
        _healthManager.onHealthPercentChange -= OnHealthPercentChange;
    }

    private void OnHealthPercentChange(float percent) => onHealthPercentChange.Invoke(percent);

    private void OnHit(int damage) => onHit.Invoke(damage);

    private void OnDeath()
    {
        onDeath.Invoke();
        OnAnyDeath?.Invoke();
    }
}
