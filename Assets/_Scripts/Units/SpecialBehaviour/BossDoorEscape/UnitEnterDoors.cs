using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

public class UnitEnterDoors : MonoBehaviour
{
    [SerializeField] private float _useCooldown;
    [SerializeField] private TweenSettings _tweenSettings;

    [Header("Door Events")]
    [SerializeField] private UnityEvent<Transform> _onDoorEnterComplete;

    private float _lastDoorUsage;

    private UnitBase _unit;

    private void Awake()
    {
        _unit = GetComponent<UnitBase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetDoors(other, out EscapeDoor doors))
            return;

        if (Time.time - _lastDoorUsage < _useCooldown)
            return;

        MoveThroughDoors(doors);
    }

    private void MoveThroughDoors(EscapeDoor doors)
    {
        _unit.Agent.enabled = false;
        _lastDoorUsage = Time.time;

        Transform entranceDoor = doors.front;

        Sequence.Create()
            .Chain(Tween.Position(_unit.transform, new(doors.doorsInside.position, _tweenSettings)))
            .ChainCallback(() => _unit.transform.position = doors.connectedDoor.doorsInside.position)
            .Chain(Tween.Position(_unit.transform, new(doors.connectedDoor.front.position, _tweenSettings)))
            .ChainCallback(() =>
            {
                _unit.Agent.enabled = true;
                _onDoorEnterComplete?.Invoke(entranceDoor);
            });
    }
    
    private bool TryGetDoors(Collider other, out EscapeDoor doors)
    {
        if (other.TryGetComponent(out EscapeDoor door))
        {
            doors = door;
            return true;
        }

        if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent(out door))
        {
            doors = door;
            return true;
        }

        doors = null;
        return false;
    }
}
