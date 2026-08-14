using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MainCharacterSO", menuName = "Scriptable Objects/MainCharacterSO")]
public class MainCharacterSO : ScriptableObject
{
    public float moveSpeed;
    public int maxHP;
    public int damage;
    public float attacksPerSecond;
    public float miningPower;


    public int carryCapacity;


    public List<AttackBase> possibleAttacks;
}
