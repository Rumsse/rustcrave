using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireflyPassiveAbility : PassiveAbility
{
    [SerializeField] private Light auraLight;
    [SerializeField] private float auraRadius;
    [SerializeField] private float updateInterval;
    [SerializeField] private float statsSharePercentage;

    private Dictionary<Unit, (float dmg, float mine)> activeBuffs = new Dictionary<Unit, (float, float)>();
    private Coroutine auraCoroutine;

    private void Start()
    {
        if (auraLight != null)
        {
            auraLight.enabled = false;
            auraLight.range = auraRadius;
        }
        auraCoroutine = StartCoroutine(AuraRoutine());
    }

    private IEnumerator AuraRoutine()
    {
        while (true)
        {
            if (unit != null && unit.enabled && unit.Agent.velocity.magnitude > 0.1f)
            {
                if (auraLight != null) auraLight.enabled = true;
                unit.IsEnergyDrainDoubled = true;

                ApplyAura();
            }
            else
            {
                if (auraLight != null) auraLight.enabled = false;
                if (unit != null) unit.IsEnergyDrainDoubled = false;
                RemoveAllBuffs();
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void ApplyAura()
    {
        float currentDmgBuff = statsManager.Damage * statsSharePercentage;
        float currentMineBuff = statsManager.MiningPower * statsSharePercentage;

        Collider[] colliders = Physics.OverlapSphere(transform.position, auraRadius);
        HashSet<Unit> unitsInRange = new HashSet<Unit>();

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            if (col.TryGetComponent<Unit>(out Unit otherUnit))
            {
                if (otherUnit.IsMainCharacter) continue;
                unitsInRange.Add(otherUnit);
            }
        }

        List<Unit> toRemove = new List<Unit>();
        foreach (var buffedUnit in activeBuffs.Keys)
        {
            if (!unitsInRange.Contains(buffedUnit) || !buffedUnit.enabled || buffedUnit == null)
            {
                toRemove.Add(buffedUnit);
            }
        }

        foreach (var rUnit in toRemove)
        {
            RemoveBuffFromUnit(rUnit);
        }

        foreach (var inRangeUnit in unitsInRange)
        {
            if (activeBuffs.ContainsKey(inRangeUnit))
            {
                var oldBuff = activeBuffs[inRangeUnit];
                if (oldBuff.dmg != currentDmgBuff || oldBuff.mine != currentMineBuff)
                {
                    RemoveBuffFromUnit(inRangeUnit);
                    AddBuffToUnit(inRangeUnit, currentDmgBuff, currentMineBuff);
                }
            }
            else
            {
                AddBuffToUnit(inRangeUnit, currentDmgBuff, currentMineBuff);
            }
        }
    }

    private void AddBuffToUnit(Unit targetUnit, float dmg, float mine)
    {
        if (targetUnit.TryGetComponent<StatsManager>(out var targetStats))
        {
            targetStats.AddFlatStatModifier(StatsType.Damage, dmg);
            targetStats.AddFlatStatModifier(StatsType.MiningPower, mine);
            targetUnit.RefreshStats();
            activeBuffs[targetUnit] = (dmg, mine);
        }
    }

    private void RemoveBuffFromUnit(Unit targetUnit)
    {
        if (targetUnit != null && activeBuffs.TryGetValue(targetUnit, out var buff))
        {
            if (targetUnit.TryGetComponent<StatsManager>(out var targetStats))
            {
                targetStats.RemoveFlatStatModifier(StatsType.Damage, buff.dmg);
                targetStats.RemoveFlatStatModifier(StatsType.MiningPower, buff.mine);
                targetUnit.RefreshStats();
            }
        }
        activeBuffs.Remove(targetUnit);
    }

    private void RemoveAllBuffs()
    {
        List<Unit> currentKeys = new List<Unit>(activeBuffs.Keys);
        foreach (var buffedUnit in currentKeys)
        {
            RemoveBuffFromUnit(buffedUnit);
        }
        activeBuffs.Clear();
    }

    private void OnDisable()
    {
        RemoveAllBuffs();
        if (auraLight != null) auraLight.enabled = false;
        if (unit != null) unit.IsEnergyDrainDoubled = false;
        if (auraCoroutine != null) StopCoroutine(auraCoroutine);
    }
}