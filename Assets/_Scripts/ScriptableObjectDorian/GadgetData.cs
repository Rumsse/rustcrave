using UnityEngine;

[CreateAssetMenu(fileName = "GadgetData", menuName = "Swarm/Inventory/GadgetData")]
public class GadgetData : ItemData
{
    public StatsType modifiedStat;
    public float statIncreaseAmount;

    public Sprite gadgetIcon;

    private void Awake()
    {
        itemType = ItemType.Gadget;
    }
}
