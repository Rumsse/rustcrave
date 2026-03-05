using System.Collections.Generic;
using System.Xml.Linq;
using TunnelSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "TunnelPrefabPool", menuName = "Tunnel System/Prefab Pool")]
public class TunnelPrefabPool : ScriptableObject
{
    [SerializeField] private TunnelType poolType;
    [SerializeField] private List<TunnelSegment> prefabs = new List<TunnelSegment>();

    public TunnelType PoolType => poolType;
    public int PrefabCount => prefabs.Count;

    public TunnelSegment GetRandomPrefab()
    {
        if (prefabs.Count == 0)
        {
            Debug.LogError($"Prefab pool {name} is empty!");
            return null;
        }

        return prefabs[Random.Range(0, prefabs.Count)];
    }

    public bool Validate()
    {
        if (prefabs.Count == 0)
        {
            Debug.LogWarning($"Pool {name} has no prefabs assigned!");
            return false;
        }

        foreach (var prefab in prefabs)
        {
            if (prefab == null)
            {
                Debug.LogError($"Pool {name} contains null prefab!");
                return false;
            }

            if (prefab.TunnelType != poolType)
                Debug.LogWarning($"Prefab {prefab.name} type mismatch in pool {name}");
        }

        return true;
    }
}