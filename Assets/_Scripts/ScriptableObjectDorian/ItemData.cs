using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Swarm/Inventory/ItemData")]
public abstract class ItemData : ScriptableObject
{
    public Sprite itemSprite;
    public Color itemColor;
    public GameObject prefab;
    public ItemType itemType;

    [TextArea(5,20)]
    public string description;
}

public enum ItemType
{
    None,
    Ore,
    Gadget
}
