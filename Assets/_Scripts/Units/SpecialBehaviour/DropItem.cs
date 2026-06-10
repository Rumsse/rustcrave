using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class DropItem : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float _dropChance;
    [SerializeField] private List<DropInstance> _possibleDrops = new();
    [SerializeField] private int _dropAmount = 1;

    [SerializeField] private float minDropRadius = 1.0f;
    [SerializeField] private float maxDropRadius = 2.0f;
    [SerializeField] private float navMeshSampleDistance = 2.0f;

    public void Drop()
    {
        if (_possibleDrops.Count == 0) return;
        if (_dropChance != 1f && Random.Range(0f, 1f) <= _dropChance) return;

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

        pickUp.SpawnDrop(transform.position, GetTargetPosition());
    }

    //here extra for dropping what was stolen 
    private void DropStolenItem()
    {
        Debug.Log("Dropping stolen item");

        gameObject.TryGetComponent(out EnemyUnit enemyUnit);

        if (!enemyUnit || !enemyUnit.StolenItem) return;

        var pickUp = Instantiate(gameObject.AddComponent<OrePickUp>(), transform.position, Quaternion.identity);
        pickUp.item = enemyUnit.StolenItem;

        pickUp.SpawnDrop(transform.position, GetTargetPosition());

        enemyUnit.ClearStolenItem();
    }

    private Vector3 GetTargetPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDropRadius, maxDropRadius);
        Vector3 rawDropPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        Vector3 targetPosition = transform.position;

        if (NavMesh.SamplePosition(rawDropPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            targetPosition = hit.position;

        return targetPosition;
    }
}

[Serializable]
public struct DropInstance
{
    public OreSO ore;
    public OrePickUp pickUpPrefab;
}