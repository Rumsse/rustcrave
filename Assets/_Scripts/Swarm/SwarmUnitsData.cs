using System;
using UnityEngine;

[Serializable]
public class SwarmUnitsData
{
    public string id;
    public UnitSO unitType;
    public int currentHP;
    public float currentEnergy;
    public bool isAlive;


    public SwarmUnitsData(UnitSO type)
    {
        id = Guid.NewGuid().ToString();
        unitType = type;
        currentHP = type.maxHP;
        currentEnergy = type.maxEnergy;
        isAlive = true;
    }

    public void RestoreEnergy(float amount)
    {
        if (!isAlive || unitType == null) return;

        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, unitType.maxEnergy);
    }

    public void RestoreHealth(int amount)
    {
        if (!isAlive || unitType == null) return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, unitType.maxHP);
    }

}