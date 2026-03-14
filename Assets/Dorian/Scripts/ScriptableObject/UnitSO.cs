using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
public class UnitSO : ScriptableObject
{
    public UnitType unitType;

    [Header("Prefab")]
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    [Header("Robot Informations")]
    public string robotName;
    public Sprite robotSprite;

    [Header("Common Stats")]
    public float moveSpeed;
    public int maxEnergy;
    public int maxHP;
    public AttackType immunities;

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
    Toter,
    MC
}


