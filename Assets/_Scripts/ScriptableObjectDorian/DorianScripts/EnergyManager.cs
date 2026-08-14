using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public event Action<float> onEnergyPercentChange;
    public event Action onEnergyDepleted;

    public float CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;

    [SerializeField] private UnitData stats;

    [Header("Energy Drain")]
    [SerializeField] private float idleDrain;
    [SerializeField] private float moveDrain;
    [SerializeField] private float actionDrain;

    private int maxEnergy;
    private float currentEnergy;
    private float currentDrain;
    float inverseMaxEnergy;
    bool isDepleted;

    private void Awake()
    {
        maxEnergy = stats.maxEnergy;
        currentEnergy = maxEnergy;
        inverseMaxEnergy = 1f / maxEnergy;
        currentDrain = idleDrain;
    }

    public void InitializeEnergy(float savedEnergy)
    {
        currentEnergy = savedEnergy;

        if (currentEnergy > 0f)
            return;

        isDepleted = true;
        onEnergyDepleted?.Invoke();
    }

    private void Update()
    {
        if (isDepleted)
            return;

        DrainEnergy();
    }

    private void DrainEnergy()
    {
        currentEnergy -= currentDrain * Time.deltaTime;

        if (currentEnergy <= 0f)
        {
            currentEnergy = 0f;
            isDepleted = true;
            onEnergyPercentChange?.Invoke(0f);
            onEnergyDepleted?.Invoke();
            return;
        }

        onEnergyPercentChange?.Invoke(currentEnergy * inverseMaxEnergy);
    }

    public void SetIdleDrain()
    {
        currentDrain = idleDrain;
    }

    public void SetMoveDrain()
    {
        currentDrain = moveDrain;
    }

    public void SetActionDrain()
    {
        currentDrain = actionDrain;
    }
}