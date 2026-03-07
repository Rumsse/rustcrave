using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public abstract class ItemSO : ScriptableObject
{
    public GameObject prefab;
    public ItemType itemType;

    [TextArea(5,20)]
    public string description;
}

public enum ItemType
{
    None,
    Ore
}
