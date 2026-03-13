using UnityEngine;

public class UnitAffectable : MonoBehaviour, IAffectable
{
    private UnitBase _unit;

    private void Awake()
    {
        _unit = GetComponent<UnitBase>();
    }

    public void ApplyEffect(EffectBase effect)
    {
        effect.ApplyEffect(_unit);
    }
}
