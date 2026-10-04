using UnityEngine;

[CreateAssetMenu(fileName = "OreData", menuName = "Swarm/Inventory/OreData")]
public class OreData : ItemData
{
    public float oreDurability;
    public string oreName;

    private void Awake()
    {
        itemType = ItemType.Ore;
    }
}
