using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MainCharacterData", menuName = "Swarm/MainCharacterData")]
public class MainCharacterData : ScriptableObject
{
    public float moveSpeed;
    public int maxHP;
    public int damage;
    public float attacksPerSecond;
    public float miningPower;


    public int carryCapacity;


    public List<AttackBase> possibleAttacks;
}
