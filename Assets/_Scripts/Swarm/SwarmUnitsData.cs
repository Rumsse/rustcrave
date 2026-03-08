using System;

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
}