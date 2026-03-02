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
        Debug.Log($"{gameObject.name} happen");
        effect.ApplyEffect(_unit);
    }
}
