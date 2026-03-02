using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StatsManager : MonoBehaviour
{
    [Header("Data Source")] [SerializeField] private UnitSO _baseData;

    #region Properties

    public UnitType UnitType => _baseData.unitType;
    public AttackType Immunities => _baseData.immunities;
    public int MaxHP => Mathf.RoundToInt(_baseData.maxHP * GetStatModifier(StatsType.MaxHP));
    public int CurrentHP => _currentHP;
    public float MoveSpeed => _currentMoveSpeed * GetStatModifier(StatsType.Speed);
    public int Damage => Mathf.RoundToInt(_currentDamage * GetStatModifier(StatsType.Damage));
    public float AttacksPerSecond => _currentAttacksPerSecond * GetStatModifier(StatsType.AttacksPerSecond);
    public float Energy => _currentEnergy * GetStatModifier(StatsType.Energy);
    public float MiningPower => _currentMiningPower *  GetStatModifier(StatsType.MiningPower);
    public int CarryCapacity => Mathf.RoundToInt(_currentCarryCapacity * GetStatModifier(StatsType.CarryCapacity));
    public List<AttackBase> PossibleAttacks => _baseData.possibleAttacks;
    
    #endregion

    #region Private Fields

    private int _currentHP;
    private float _currentMoveSpeed;
    private int _currentDamage;
    private float _currentAttacksPerSecond;
    private float _currentEnergy;
    private float _currentMiningPower;
    private int _currentCarryCapacity;

    private Dictionary<StatsType, float> _statsModifiers = new();
    
    #endregion

    private void OnEnable()
    {
        if (_baseData != null) { Initialize(_baseData); }
    }

    public void Initialize(UnitSO data)
    {
        _baseData = data;

        _statsModifiers.Clear();
        foreach (StatsType type in Enum.GetValues(typeof(StatsType)))
        {
            _statsModifiers[type] = 1.0f;
        }
        
        _currentHP = _baseData.maxHP;
        _currentMoveSpeed = _baseData.moveSpeed;
        _currentDamage = _baseData.damage;
        _currentAttacksPerSecond = _baseData.attacksPerSecond;
        _currentEnergy = _baseData.energy;
        _currentMiningPower = _baseData.miningPower;
        _currentCarryCapacity = _baseData.carryCapacity;
    }

    public void AddTimerStatModifier(StatsType stat, float modifier, float duration)
    {
        if(modifier != 0)
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

    [ContextMenu("xd")]
    public void GetSpeedStat()
    {
        Debug.Log($"{gameObject.name} {GetStatModifier(StatsType.Speed)}");
    }

    public void ChangeStats(UnitSO stats) => _baseData = stats;
}

[Serializable] 
public enum StatsType
{
    Speed,
    Damage,
    AttacksPerSecond,
    Energy,
    MiningPower,
    CarryCapacity,
    MaxHP
}