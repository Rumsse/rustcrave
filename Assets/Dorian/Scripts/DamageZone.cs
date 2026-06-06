using System.Data.SqlTypes;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageZone : MonoBehaviour
{
    [SerializeField] private int dmg;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Unit Unit))
        {
            Unit.HealthManager.Damage(new DamageInfo(dmg, AttackType.Physical));
        }
    }
}