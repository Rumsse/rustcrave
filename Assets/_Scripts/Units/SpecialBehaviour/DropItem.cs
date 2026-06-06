using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropItem : MonoBehaviour //?
{
    [SerializeField] [Range(0f, 1f)] private float _dropChance;
    [SerializeField] private List<DropInstance> _possibleDrops = new();
    [SerializeField] private int _dropAmount = 1;

    public void Drop()
    {
        if (_possibleDrops.Count == 0) return;
        if (Random.Range(0f, 1f) <= _dropChance) return;

        DropRandomItem();
        DropStolenItem();
    }

    private void DropRandomItem()
    {
        Debug.Log("Dropping random item");

        var randomPickUp = _possibleDrops[Random.Range(0, _possibleDrops.Count)];
        var pickUp = Instantiate(randomPickUp.pickUpPrefab, transform.position, Quaternion.identity);
        pickUp.item = randomPickUp.ore;

        if (_dropAmount > 1)
        {
            pickUp.oreValueAmount = _dropAmount;
        }
    }

    //here extra for dropping what was stolen 
    private void DropStolenItem()
    {
        Debug.Log("Dropping stolen item");

        gameObject.TryGetComponent(out EnemyUnit enemyUnit);

        if (!enemyUnit || !enemyUnit.StolenItem) return;

        var pickUp = Instantiate(gameObject.AddComponent<OrePickUp>(), transform.position, Quaternion.identity);
        pickUp.item = enemyUnit.StolenItem;

        enemyUnit.ClearStolenItem();
    }
}

[Serializable]
public struct DropInstance
{
    public OreSO ore;
    public OrePickUp pickUpPrefab;
}