using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    protected Unit unit;
    protected StatsManager statsManager;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
        statsManager = GetComponent<StatsManager>();
    }
}