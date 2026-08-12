using UnityEngine;

[CreateAssetMenu(fileName = "OreSO", menuName = "Scriptable Objects/OreSO")]
public class OreSO : ItemSO
{
    public float oreDurability;
    public string oreName;

    private void Awake()
    {
        itemType = ItemType.Ore;
    }
}
