using UnityEngine;
using UnityEngine.AI;

public class OreNode : MonoBehaviour, IMineable
{
    [SerializeField] private OreSO ore;
    [SerializeField] private int amount;
    [SerializeField] private OrePickUp dropPrefab;

    [SerializeField] private float minDropRadius = 1.0f;
    [SerializeField] private float maxDropRadius = 2.0f;
    [SerializeField] private float dropHeight = 0.5f;
    [SerializeField] private float navMeshSampleDistance = 2.0f;
    [SerializeField] private int dropAmount;

    public ItemSO Mine()
    {
        if (amount <= 0)
            return null;

        amount--;

        if (dropPrefab != null)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDropRadius, maxDropRadius);
            Vector3 rawDropPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (NavMesh.SamplePosition(rawDropPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                Vector3 finalDropPosition = hit.position + new Vector3(0f, dropHeight, 0f);
                OrePickUp droppedItem = Instantiate(dropPrefab, finalDropPosition, Quaternion.identity);
                droppedItem.item = ore;
                dropAmount = droppedItem.oreValueAmount;
            }
            else
            {
                Vector3 fallbackPosition = transform.position + new Vector3(0f, dropHeight, 0f);
                OrePickUp droppedItem = Instantiate(dropPrefab, fallbackPosition, Quaternion.identity);
                droppedItem.item = ore;
                dropAmount = droppedItem.oreValueAmount;
            }
        }

        if (amount <= 0)
            Destroy(gameObject);

        return ore;
    }

    public float GetDurability()
    {
        return ore != null ? ore.oreDurability : 0f;
    }

    public bool IsDepleted()
    {
        return amount <= 0;
    }
}