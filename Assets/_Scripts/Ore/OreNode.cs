using UnityEngine;
using UnityEngine.AI;

public class OreNode : MonoBehaviour, IMineable
{
    #region Configuration

    [SerializeField] private ActiveModifier activeModifier;
    [SerializeField] private OreSO ore;
    [SerializeField] private OreTooltip oreTooltip;
    [SerializeField] private int amount;
    [SerializeField] private OrePickUp dropPrefab;
    [SerializeField] private ParticleSystem miningEffect;

    [SerializeField] private float minDropRadius = 1.0f;
    [SerializeField] private float maxDropRadius = 2.0f;
    [SerializeField] private float navMeshSampleDistance = 2.0f;
    [SerializeField] private int dropAmount;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (activeModifier != null && activeModifier.IsFragileCrust)
        {
            amount = Mathf.CeilToInt(amount * 0.5f);
        }
    }

    private void OnMouseEnter()
    {
        if (Time.timeScale == 0f)
            return;

        oreTooltip.ShowTooltip();
    }

    private void OnMouseExit() => oreTooltip.HideTooltip();

    #endregion

    #region Mining Logic

    public ItemSO Mine()
    {
        if (amount <= 0)
            return null;

        amount--;

        if (dropPrefab != null)
        {
            SpawnDropItem();
            if (activeModifier != null && activeModifier.IsVoidChase)
                SpawnDropItem();
        }

        if (amount <= 0)
            Destroy(gameObject);

        return ore;
    }

    private void SpawnDropItem()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDropRadius, maxDropRadius);
        Vector3 rawDropPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        Vector3 targetPosition = transform.position;

        if (NavMesh.SamplePosition(rawDropPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            targetPosition = hit.position;

        OrePickUp droppedItem = Instantiate(dropPrefab, transform.position, Quaternion.identity);
        droppedItem.item = ore;
        dropAmount = droppedItem.oreValueAmount;

        droppedItem.SpawnDrop(transform.position, targetPosition);
    }

    #endregion

    #region Interface Implementations

    public float GetDurability() => ore != null ? ore.oreDurability : 0f;

    public bool IsDepleted() => amount <= 0;

    public OreSO GetOreData() => ore;

    #endregion

    #region Effects

    public void PlayEffect() => miningEffect?.Play(true);

    public void StopEffect() => miningEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

    #endregion
}