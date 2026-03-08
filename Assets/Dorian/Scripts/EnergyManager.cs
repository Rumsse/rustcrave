using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public event Action<float> onEnergyPercentChange;
    public event Action onEnergyDepleted;

    public float CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;

    [SerializeField] private UnitSO stats;

    [Header("Energy Drain")]
    [SerializeField] private float idleDrain;
    [SerializeField] private float moveDrain;
    [SerializeField] private float actionDrain;

    private int maxEnergy;
    private float currentEnergy;

    private float currentDrain;

    private void Awake()
    {
        maxEnergy = stats.maxEnergy;
        currentEnergy = maxEnergy;

        currentDrain = idleDrain;
    }

    public void InitializeEnergy(float savedEnergy)
    {
        currentEnergy = savedEnergy;

        if (currentEnergy <= 0)
            onEnergyDepleted?.Invoke();
    }

    private void Update()
    {
        DrainEnergy();
    }

    private void DrainEnergy()
    {
        if (currentEnergy <= 0)
            return;

        currentEnergy -= currentDrain * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        onEnergyPercentChange?.Invoke(currentEnergy / maxEnergy);

        if (currentEnergy == 0)
            onEnergyDepleted?.Invoke();
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