using System.Data.SqlTypes;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Unit Unit))
        {
            Unit.HealthManager.Death();
            UnitRegistry.Unregister(Unit);
        }
    }
}