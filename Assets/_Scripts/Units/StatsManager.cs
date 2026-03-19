using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StatsManager : MonoBehaviour
{
    [SerializeField] private UnitSO _baseData;

    public Transform PrefabT => _baseData.Prefab.transform;
    public string RobotName => _baseData.robotName;
    public Sprite RobotSprite => _baseData.robotSprite;
    public UnitType UnitType => _baseData.unitType;
    public AttackType TypeImmunities => _baseData.typeImmunities;
    public DeliveryMethod DeliveryMethodImmunities => _baseData.deliveryMethodImmunities;
    public int MaxHP => Mathf.RoundToInt(_baseData.maxHP * GetStatModifier(StatsType.MaxHP));
    public float MoveSpeed => _baseData.moveSpeed * GetStatModifier(StatsType.Speed);
    public int Damage => Mathf.RoundToInt(_baseData.damage * GetStatModifier(StatsType.Damage));
    public float AttacksPerSecond => _baseData.attacksPerSecond * GetStatModifier(StatsType.AttacksPerSecond);
    public float MiningPower => _baseData.miningPower *  GetStatModifier(StatsType.MiningPower);
    public int CarryCapacity => Mathf.RoundToInt(_baseData.carryCapacity * GetStatModifier(StatsType.CarryCapacity));
    public List<AttackBase> PossibleAttacks => _baseData.possibleAttacks;
    public int MaxEnergy => Mathf.RoundToInt((_baseData.maxEnergy + GetFlatStatModifier(StatsType.MaxEnergy)) * GetStatModifier(StatsType.MaxEnergy));

    private Dictionary<StatsType, float> _statsModifiers = new();
    private Dictionary<StatsType, float> _flatStatsModifiers = new();

    private void OnEnable()
    {
        if (_baseData != null) { Initialize(_baseData); }
    }

    public void Initialize(UnitSO data)
    {
        _baseData = data;

        _statsModifiers.Clear();
        _flatStatsModifiers.Clear();
        foreach (StatsType type in Enum.GetValues(typeof(StatsType)))
        {
            _statsModifiers[type] = 1.0f;
            _flatStatsModifiers[type] = 0.0f;
        }
    }

    public void AddTimerStatModifier(StatsType stat, float modifier, float duration)
    {
        if (modifier != 0 && gameObject.activeInHierarchy)
            StartCoroutine(TimedModifierC(stat, modifier, duration));
    }

    private IEnumerator TimedModifierC(StatsType stat, float modifier, float duration)
    {
        AddStatModifier(stat, modifier);
        yield return new WaitForSeconds(duration);
        RemoveStatModifier(stat, modifier);
    }

    private void AddStatModifier(StatsType stat, float modifier) => _statsModifiers[stat] *= modifier;
    private void RemoveStatModifier(StatsType stat, float modifier) => _statsModifiers[stat] /= modifier;
    public float GetStatModifier(StatsType stat) => _statsModifiers.GetValueOrDefault(stat, 1f);

    public void AddFlatStatModifier(StatsType stat, float amount) => _flatStatsModifiers[stat] += amount;
    public void RemoveFlatStatModifier(StatsType stat, float amount) => _flatStatsModifiers[stat] -= amount;
    public float GetFlatStatModifier(StatsType stat) => _flatStatsModifiers.GetValueOrDefault(stat, 0f);

    public void ChangeStats(UnitSO stats) => _baseData = stats;
}

[Serializable]
public enum StatsType
{
    Speed,
    Damage,
    AttacksPerSecond,
    MaxEnergy,
    MiningPower,
    CarryCapacity,
    MaxHP
}