using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AttackBase : ScriptableObject
{
    public string attackName;
    public float damageMult; // if unit damage is 5 and this is 1.5f then attack would deal 8 after ceiling it up to int
    public string animationStateName;
    public float attackRange;
    public AttackType type;

    public abstract void Execute(UnitBase target, UnitBase attacker);
    
    protected int GetFinalDamage(int initialDamage) => Mathf.RoundToInt(initialDamage * damageMult);
}

[Flags]
public enum AttackType
{
    None = 0,
    Physical = 1 << 0,
    Magic = 1 << 1,
    Environmental = 1 << 2,
    True = 1 << 3// non blockable by anything
}

[Serializable] 
public struct DamageInfo
{
    public int Value;
    public AttackType AttackType;

    public DamageInfo(int value, AttackType type)
    {
        Value = value;
        AttackType = type;
    }
}