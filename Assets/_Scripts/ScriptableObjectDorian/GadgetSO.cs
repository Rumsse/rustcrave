using UnityEngine;

[CreateAssetMenu(fileName = "GadgetSO", menuName = "Scriptable Objects/GadgetSO")]
public class GadgetSO : ItemSO
{
    public StatsType modifiedStat;
    public float statIncreaseAmount;

    public Sprite gadgetIcon;

    private void Awake()
    {
        itemType = ItemType.Gadget;
    }
}
