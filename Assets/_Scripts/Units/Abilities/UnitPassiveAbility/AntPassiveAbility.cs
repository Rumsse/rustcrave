using UnityEngine;
using System.Collections;

public class AntPassiveAbility : PassiveAbility
{
    [SerializeField] private float checkRadius;
    [SerializeField] private float checkInterval;
    [SerializeField] private string antRobotName;
    [SerializeField] private float baseBuffAmount;
    [SerializeField] private float antBonusBuffAmount;

    private int currentBuffLevel;
    private Coroutine checkCoroutine;

    private void Start()
    {
        checkCoroutine = StartCoroutine(CheckProximityRoutine());
    }

    private IEnumerator CheckProximityRoutine()
    {
        while (true)
        {
            if (unit != null && unit.enabled)
            {
                EvaluateBuff();
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void EvaluateBuff()
    {
        int foundLevel = 0;
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius);

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            if (col.TryGetComponent<Unit>(out Unit otherUnit))
            {
                if (otherUnit.IsMainCharacter) continue;

                foundLevel = 1;

                if (otherUnit.TryGetComponent<StatsManager>(out var otherStats))
                {
                    if (otherStats.RobotName == antRobotName)
                    {
                        foundLevel = 2;
                        break;
                    }
                }
            }
        }

        if (foundLevel != currentBuffLevel)
        {
            ApplyBuff(foundLevel);
        }
    }

    private void ApplyBuff(int newLevel)
    {
        if (currentBuffLevel > 0)
        {
            float amountToRemove = currentBuffLevel == 1 ? baseBuffAmount : baseBuffAmount + antBonusBuffAmount;
            statsManager.RemoveFlatStatModifier(StatsType.Speed, amountToRemove);
            statsManager.RemoveFlatStatModifier(StatsType.Damage, amountToRemove);
            statsManager.RemoveFlatStatModifier(StatsType.MiningPower, amountToRemove);
        }

        currentBuffLevel = newLevel;

        if (currentBuffLevel > 0)
        {
            float amountToAdd = currentBuffLevel == 1 ? baseBuffAmount : baseBuffAmount + antBonusBuffAmount;
            statsManager.AddFlatStatModifier(StatsType.Speed, amountToAdd);
            statsManager.AddFlatStatModifier(StatsType.Damage, amountToAdd);
            statsManager.AddFlatStatModifier(StatsType.MiningPower, amountToAdd);
        }

        unit.RefreshStats();
    }

    private void OnDisable()
    {
        if (currentBuffLevel > 0 && statsManager != null)
        {
            ApplyBuff(0);
        }
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }
    }
}