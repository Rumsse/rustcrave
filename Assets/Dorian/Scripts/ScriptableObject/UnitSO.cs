using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
public class UnitSO : ScriptableObject
{
    public UnitType unitType;

    [Header("Common Stats")]
    public float moveSpeed;
    public int maxHP;
    public float energy;

    [Header("Warrior")]
    public int damage;
    public float attacksPerSecond;

    [Header("Miner")]
    public float miningPower;
    
    [Header("Toter")]
    public int carryCapacity;

    public List<AttackBase> possibleAttacks;

}

public enum UnitType
{
    Warrior,
    Miner,
    Toter
}


