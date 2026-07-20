using UnityEngine;

[RequireComponent(typeof(DropItem))]
public class ChestInteraction : InteractableBase
{
    [SerializeField] private int minDrops = 1;
    [SerializeField] private int maxDrops = 3;

    private DropItem dropItem;

    private void Awake() => dropItem = GetComponent<DropItem>();

    public override void OnInteract()
    {
        int randomDropCount = Random.Range(minDrops, maxDrops + 1);

        for (int i = 0; i < randomDropCount; i++)
            dropItem.Drop();

        Destroy(gameObject);
    }
}