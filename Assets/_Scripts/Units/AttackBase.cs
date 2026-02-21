using Unity.VisualScripting;
using UnityEngine;

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

public enum AttackType
{
    Physical,
    Magic,
    True // non blockable by anything
}