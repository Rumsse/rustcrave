using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class DropItem : MonoBehaviour
{
    [SerializeField] private ActiveModifier activeModifier;
    [SerializeField][Range(0f, 1f)] private float _dropChance;
    [SerializeField] private List<DropInstance> _possibleDrops = new();
    [SerializeField] private int _dropAmount = 1;

    [SerializeField] private float minDropRadius = 1.0f;
    [SerializeField] private float maxDropRadius = 2.0f;
    [SerializeField] private float navMeshSampleDistance = 2.0f;

    public void Drop()
    {
        if (_possibleDrops.Count == 0) return;

        float finalChance = _dropChance;
        int finalAmount = _dropAmount;

        if (activeModifier != null && activeModifier.IsVoidChase)
        {
            finalChance = 1f;
            finalAmount *= 2;
        }

        if (finalChance < 1f && Random.value > finalChance) return;

        DropRandomItems();
        DropStolenItem();
    }

    private void DropRandomItems()
    {
        Debug.Log("Dropping random items");

        var randomPickUp = _possibleDrops[Random.Range(0, _possibleDrops.Count)];

        for (int i = 0; i < _dropAmount; i++)
        {
            var pickUp = Instantiate(randomPickUp.pickUpPrefab, transform.position, Quaternion.identity);
            pickUp.item = randomPickUp.ore;
            pickUp.SpawnDrop(transform.position, GetTargetPosition());
        }
    }

    //here extra for dropping what was stol
    private void DropStolenItem()
    {
        Debug.Log("Dropping stolen item");

        if (!gameObject.TryGetComponent(out EnemyUnit enemyUnit) || !enemyUnit.StolenItem) return;

        var pickUp = Instantiate(gameObject.AddComponent<OrePickUp>(), transform.position, Quaternion.identity);
        pickUp.item = enemyUnit.StolenItem;

        pickUp.SpawnDrop(transform.position, GetTargetPosition());

        enemyUnit.ClearStolenItem();
    }

    private Vector3 GetTargetPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDropRadius, maxDropRadius);
        Vector3 rawDropPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(rawDropPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }
}

[Serializable]
public struct DropInstance
{
    public OreData ore;
    public OrePickUp pickUpPrefab;
}