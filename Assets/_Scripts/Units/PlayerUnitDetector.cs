using UnityEngine;

public class PlayerUnitDetector : MonoBehaviour
{
    [SerializeField] protected EnemyUnit unit;
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!unit || !unit.FarDetectEnabled()) return;
        
        if (!other.CompareTag("Unit"))
            return;

        if (!other.TryGetComponent(out Unit unitObj))
            return;
        
        unit.UnitEnter(unitObj);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!unit) return;

        if (!other.CompareTag("Unit"))
            return;
        
        if (!other.TryGetComponent(out Unit unitObj))
            return;
        
        unit.UnitLeave(unitObj);
    }
}
