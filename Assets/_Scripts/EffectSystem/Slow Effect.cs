using System;
using UnityEngine;

[Serializable]
public class SlowEffect : EffectBase
{
    public float modifier;

    public override void ApplyEffect(UnitBase target)
    {
        Debug.Log($"happend");
        target.Stats.AddTimerStatModifier(StatsType.Speed, modifier, duration);
    }
}
