using UnityEngine;

public class OreNode : MonoBehaviour, IMineable
{
    [SerializeField] private OreSO ore;
    [SerializeField] private int amount;

    public ItemSO Mine()
    {
        if (amount <= 0)
            return null;

        amount--;
        Debug.Log("Amount is: " +  amount);

        if (amount == 0)
            Destroy(gameObject);


        return ore;
    }
}
