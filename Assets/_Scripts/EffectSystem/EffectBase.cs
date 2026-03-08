using System;
using UnityEngine;

[Serializable]
public abstract class EffectBase
{
    public float duration;
    
    public abstract void ApplyEffect(UnitBase target);
}
