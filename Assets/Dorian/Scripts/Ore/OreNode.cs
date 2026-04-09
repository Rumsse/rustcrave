using UnityEngine;
using UnityEngine.AI;

public class OreNode : MonoBehaviour, IMineable
{
    [SerializeField] private OreSO ore;
    [SerializeField] private OreTooltip oreTooltip;
    [SerializeField] private int amount;
    [SerializeField] private OrePickUp dropPrefab;
    [SerializeField] private ParticleSystem miningEffect;

    [SerializeField] private float minDropRadius = 1.0f;
    [SerializeField] private float maxDropRadius = 2.0f;
    [SerializeField] private float navMeshSampleDistance = 2.0f;
    [SerializeField] private int dropAmount;

    public void PlayEffect()
    {
        if (miningEffect != null)
            miningEffect.Play(true);
    }

    public void StopEffect()
    {
        if (miningEffect != null)
            miningEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public ItemSO Mine()
    {
        if (amount <= 0)
            return null;

        amount--;

        if (dropPrefab != null)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDropRadius, maxDropRadius);
            Vector3 rawDropPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 spawnPosition = transform.position;

            if (NavMesh.SamplePosition(rawDropPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                spawnPosition = hit.position;

            OrePickUp droppedItem = Instantiate(dropPrefab, spawnPosition, Quaternion.identity);
            droppedItem.item = ore;
            dropAmount = droppedItem.oreValueAmount;
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

    private void OnMouseEnter()
    {
        oreTooltip.ShowTooltip();
    }

    private void OnMouseExit()
    {
        oreTooltip.HideTooltip();
    }
}