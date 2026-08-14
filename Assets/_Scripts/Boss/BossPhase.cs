using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossPhase", menuName = "Boss/Boss Phase")]
public class BossPhase : ScriptableObject
{
    public float healthPercent;
    public string animationTrigger;
    public UnitData newStats;
}
