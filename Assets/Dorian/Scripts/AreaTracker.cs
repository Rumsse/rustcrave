using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class AreaTracker<T> : MonoBehaviour
{
    protected readonly HashSet<T> entitiesInZone = new HashSet<T>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out T entity))
        {
            entitiesInZone.Add(entity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out T entity))
        {
            entitiesInZone.Remove(entity);
        }
    }

    protected virtual void OnDisable()
    {
        entitiesInZone.Clear();
    }
}