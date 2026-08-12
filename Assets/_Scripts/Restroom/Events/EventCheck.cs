using System;
using UnityEngine;

[Serializable]
public abstract class EventCheck
{
    [Range(0, 100)] public int baseSuccessChance = 50;

    public abstract int GetFinalChance(SwarmUnitsData robot);
}

[Serializable]
public class LuckCheck : EventCheck
{
    public override int GetFinalChance(SwarmUnitsData robot) => baseSuccessChance;
}

[Serializable]
public class StatCheck : EventCheck
{
    public StatsType stat;
    public int bonusPerPoint = 10;

    public override int GetFinalChance(SwarmUnitsData robot)
    {
        if (robot == null)
            return baseSuccessChance;

        float statValue = stat switch
        {
            StatsType.Speed => robot.GetTotalSpeed(),
            StatsType.Damage => robot.GetTotalDamage(),
            StatsType.AttacksPerSecond => robot.unitType ? robot.unitType.attacksPerSecond : 0,
            StatsType.MaxEnergy => robot.GetTotalMaxEnergy(),
            StatsType.MiningPower => robot.GetTotalMiningPower(),
            StatsType.CarryCapacity => robot.GetTotalCapacity(),
            StatsType.MaxHP => robot.GetTotalMaxHP(),
            _ => 0f
        };

        return baseSuccessChance + Mathf.RoundToInt(statValue * bonusPerPoint);
    }
}
