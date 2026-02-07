using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
public class UnitSO : ScriptableObject
{
    public float moveSpeed;
    public int maxHP;
    public int damage;
    public float attacksPerSecond;

    public List<AttackBase> possibleAttacks;
}
