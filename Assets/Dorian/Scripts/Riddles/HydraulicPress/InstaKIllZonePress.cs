using System.Data.SqlTypes;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InstaKillZonePress : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        IKillable target = other.GetComponent<IKillable>();
        if (target != null)
        {
            target?.Kill();
        }
    }
}