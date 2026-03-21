using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
public class UnitSO : ScriptableObject
{
    public UnitType unitType;

    [Header("Prefab")]
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    [Header("Robot Informations")]
    public string robotName;
    [TextArea]
    public string robotDescription;
    public Sprite robotSprite;
    [TextArea]
    public string abilityDescription;

    [Header("Common Stats")]
    public float moveSpeed;
    public int maxEnergy;
    public int maxHP;
    [FormerlySerializedAs("immunities")] public AttackType typeImmunities;
    public DeliveryMethod deliveryMethodImmunities;

    [Header("Warrior")]
    public int damage;
    public float attacksPerSecond;

    [Header("Miner")]
    public float miningPower;
    
    [Header("Toter")]
    public int carryCapacity;

    public List<AttackBase> possibleAttacks;

    public UnitSounds sounds;

}

public enum UnitType
{
    Warrior,
    Miner,
    Toter,
    MC
}

[Serializable]
public struct UnitSounds
{
    // todo: later probably should move attackSound to attackSO
    public EventReference attackSound;
    public EventReference walkSound;
    public EventReference mineSound;
    public EventReference selectSound;
    public EventReference takeDamageSound;
    public EventReference deathSound;
    public EventReference commandSound;
}
