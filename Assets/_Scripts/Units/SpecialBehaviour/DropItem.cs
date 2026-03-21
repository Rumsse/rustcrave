using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropItem : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float _dropChance;
    [SerializeField] private List<DropInstance> _possibleDrops = new();

    public void Drop()
    {
        if (_possibleDrops.Count == 0) return;
        if (Random.Range(0f, 1f) <= _dropChance) return;
        
        var randomPickUp = _possibleDrops[Random.Range(0, _possibleDrops.Count)];
        var pickUp = Instantiate(randomPickUp.pickUpPrefab, transform.position, Quaternion.identity);
        pickUp.item = randomPickUp.ore;
    }
}

[Serializable]
public struct DropInstance
{
    public OreSO ore;
    public OrePickUp pickUpPrefab;
}