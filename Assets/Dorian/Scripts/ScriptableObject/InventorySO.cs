using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Objects/InventorySO")]
public class InventorySO : ScriptableObject
{
    public List<InventorySlot> inventoryItemList = new List<InventorySlot>();
    public void AddItem(ItemSO item, int amount)
    {
        bool hasItem = false;
        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            if (inventoryItemList[i].item == item) 
            {
                inventoryItemList[i].AddAmount(amount);
                hasItem = true;
                break;
            }
        }
        if (!hasItem)
        {
            inventoryItemList.Add(new InventorySlot(item, amount));
        }
    }
}


[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int amount;
    public InventorySlot(ItemSO item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public void AddAmount(int value)
    {
        amount += value;
    }
}
