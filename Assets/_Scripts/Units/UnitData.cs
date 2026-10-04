using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

public enum UnitRole
{
    Warrior,
    Miner,
    Specialist,
    Conductor
}

[CreateAssetMenu(fileName = "UnitData", menuName = "Swarm/Unit Data")]
public class UnitData : ScriptableObject
{
    #region General

    [FormerlySerializedAs("unitType")]
    public UnitRole unitRole;

    [FormerlySerializedAs("prefab")]
    [SerializeField] private GameObject unitPrefab;
    public GameObject UnitPrefab => unitPrefab;

    [FormerlySerializedAs("robotSprite")]
    public Sprite unitIcon;

    public bool isMainCharacter;

    [FormerlySerializedAs("robotName")]
    public string unitName;

    [FormerlySerializedAs("robotDescription")]
    [TextArea(5, 5)] public string unitDescription;

    [TextArea(5, 5)] public string abilityDescription;

    #endregion

    #region Common Stats

    public int maxHP;
    public int maxEnergy;
    public float moveSpeed;
    public int damage;
    public float attacksPerSecond;
    public float miningPower;
    public int carryCapacity;

    [FormerlySerializedAs("immunities")]
    public AttackType typeImmunities;

    [FormerlySerializedAs("deliveryMethodImmunities")]
    public DeliveryMethod deliveryImmunities;

    public List<AttackBase> possibleAttacks;

    #endregion

    #region Audio

    [FormerlySerializedAs("sounds")]
    public UnitAudio audioSettings;

    #endregion
}

[Serializable]
public struct OreMiningSounds
{
    public OreData ore;
    public EventReference sound;
}

[Serializable]
public struct UnitAudio
{
    public EventReference attackSound;
    public EventReference mineSound;
    public List<OreMiningSounds> oreMiningSounds;
    public EventReference selectSound;
    public EventReference takeDamageSound;
    public EventReference deathSound;
    public EventReference commandSound;
    public EventReference orderSound;
}