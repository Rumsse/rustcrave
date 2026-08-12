using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireflyPassiveAbility : PassiveAbility
{
    [SerializeField] private Light auraLight;
    [SerializeField] private float auraRadius;
    [SerializeField] private float updateInterval;
    [SerializeField] private float statsSharePercentage;
    [SerializeField] private float lightFadeSpeed = 5f;

    private Dictionary<Unit, (float dmg, float mine)> activeBuffs = new Dictionary<Unit, (float, float)>();
    private Coroutine auraCoroutine;
    private float maxIntensity;
    private float targetIntensity;

    #region Unity Lifecycle

    private void Start()
    {
        if (auraLight != null)
        {
            maxIntensity = auraLight.intensity;
            auraLight.intensity = 0f;
            auraLight.enabled = false;
        }

        auraCoroutine = StartCoroutine(AuraRoutine());
    }

    private void Update()
    {
        if (auraLight == null)
            return;

        auraLight.intensity = Mathf.MoveTowards(auraLight.intensity, targetIntensity, lightFadeSpeed * Time.deltaTime);

        if (auraLight.intensity > 0f && !auraLight.enabled)
            auraLight.enabled = true;
        else if (auraLight.intensity == 0f && auraLight.enabled)
            auraLight.enabled = false;
    }

    private void OnDisable()
    {
        RemoveAllBuffs();

        if (auraLight != null)
            auraLight.enabled = false;

        if (unit != null)
            unit.IsEnergyDrainDoubled = false;

        if (auraCoroutine != null)
            StopCoroutine(auraCoroutine);
    }

    #endregion

    #region Aura Logic

    private IEnumerator AuraRoutine()
    {
        while (true)
        {
            if (unit == null || !unit.enabled)
            {
                SetAuraState(false);
                yield return new WaitForSeconds(updateInterval);
                continue;
            }

            if (unit.Agent.velocity.magnitude > 0.1f)
            {
                SetAuraState(true);
                ApplyAura();
            }
            else
            {
                SetAuraState(false);
                RemoveAllBuffs();
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void SetAuraState(bool isActive)
    {
        targetIntensity = isActive ? maxIntensity : 0f;

        if (unit != null)
            unit.IsEnergyDrainDoubled = isActive;
    }

    #endregion

    #region Buffs Handling

    private void ApplyAura()
    {
        float currentDmgBuff = statsManager.Damage * statsSharePercentage;
        float currentMineBuff = statsManager.MiningPower * statsSharePercentage;

        Collider[] colliders = Physics.OverlapSphere(transform.position, auraRadius);
        HashSet<Unit> unitsInRange = new HashSet<Unit>();

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject)
                continue;

            if (!col.TryGetComponent<Unit>(out Unit otherUnit))
                continue;

            if (otherUnit.IsMainCharacter)
                continue;

            unitsInRange.Add(otherUnit);
        }

        List<Unit> toRemove = new List<Unit>();

        foreach (var buffedUnit in activeBuffs.Keys)
        {
            if (!unitsInRange.Contains(buffedUnit) || !buffedUnit.enabled || buffedUnit == null)
                toRemove.Add(buffedUnit);
        }

        foreach (var rUnit in toRemove)
            RemoveBuffFromUnit(rUnit);

        foreach (var inRangeUnit in unitsInRange)
        {
            if (activeBuffs.TryGetValue(inRangeUnit, out var oldBuff))
            {
                if (oldBuff.dmg == currentDmgBuff && oldBuff.mine == currentMineBuff)
                    continue;

                RemoveBuffFromUnit(inRangeUnit);
            }

            AddBuffToUnit(inRangeUnit, currentDmgBuff, currentMineBuff);
        }
    }

    private void AddBuffToUnit(Unit targetUnit, float dmg, float mine)
    {
        if (!targetUnit.TryGetComponent<StatsManager>(out var targetStats))
            return;

        targetStats.AddFlatStatModifier(StatsType.Damage, dmg);
        targetStats.AddFlatStatModifier(StatsType.MiningPower, mine);
        targetUnit.RefreshStats();
        activeBuffs[targetUnit] = (dmg, mine);
    }

    private void RemoveBuffFromUnit(Unit targetUnit)
    {
        if (targetUnit == null || !activeBuffs.TryGetValue(targetUnit, out var buff))
            return;

        if (targetUnit.TryGetComponent<StatsManager>(out var targetStats))
        {
            targetStats.RemoveFlatStatModifier(StatsType.Damage, buff.dmg);
            targetStats.RemoveFlatStatModifier(StatsType.MiningPower, buff.mine);
            targetUnit.RefreshStats();
        }

        activeBuffs.Remove(targetUnit);
    }

    private void RemoveAllBuffs()
    {
        List<Unit> currentKeys = new List<Unit>(activeBuffs.Keys);

        foreach (var buffedUnit in currentKeys)
            RemoveBuffFromUnit(buffedUnit);

        activeBuffs.Clear();
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }

    #endregion
}