using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitDetector : MonoBehaviour
{
    [SerializeField] private EnemyUnit unit;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Unit"))
            return;

        if (!other.TryGetComponent(out Unit unitObj))
            return;
        
        unit.UnitEnter(unitObj);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Unit"))
            return;
        
        if (!other.TryGetComponent(out Unit unitObj))
            return;
        
        unit.UnitLeave(unitObj);
    }
}
