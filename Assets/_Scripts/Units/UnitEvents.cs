using UnityEngine;
using UnityEngine.Events;

public class UnitEvents : MonoBehaviour
{
    public UnityEvent onAttack;
    public UnityEvent onSpecialStart;
    public UnityEvent onSpecialEnd;
    public UnityEvent<UnitState> onStateChange;
    
    private UnitBase _unit;

    private void Awake()
    {
        _unit = GetComponent<UnitBase>();
    }

    private void OnEnable()
    {
        _unit.onAttack += OnAttack;
        _unit.onSpecialStart += OnSpecialStart;
        _unit.onSpecialStop += OnSpecialStop;
        _unit.onStateChange += OnStateChange;
    }

    private void OnDisable()
    {
        _unit.onAttack -= OnAttack;
        _unit.onSpecialStart -= OnSpecialStart;
        _unit.onSpecialStop -= OnSpecialStop;
        _unit.onStateChange -= OnStateChange;
    }

    private void OnStateChange(UnitState obj) => onStateChange.Invoke(obj);
    private void OnSpecialStop() =>  onSpecialEnd.Invoke();
    private void OnSpecialStart() =>  onSpecialStart.Invoke();
    private void OnAttack() => onAttack.Invoke();
}
