using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SteamArea : MonoBehaviour
{
    [SerializeField] private Vector3 pushDirection = Vector3.down;
    [SerializeField] private float pushForce = 10f;

    private readonly HashSet<IPushable> entitiesInZone = new HashSet<IPushable>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IPushable pushable))
            entitiesInZone.Add(pushable);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IPushable pushable))
            entitiesInZone.Remove(pushable);
    }

    private void Update()
    {
        foreach (var entity in entitiesInZone)
        {
            if (!entity.CanBePushed)
                continue;

            entity.ApplyPush(pushDirection.normalized, pushForce * Time.deltaTime);
        }
    }

    private void OnDisable() => entitiesInZone.Clear();
}