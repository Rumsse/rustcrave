using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Objects/InventorySO")]
public class InventorySO : ScriptableObject
{
    public int maxCapacity;

    public List<InventorySlot> inventoryItemList = new List<InventorySlot>();

    public event EventHandler OnInventoryChanged;

    public bool AddItem(ItemSO item, int amount)
    {
        int currentAmount = GetTotalAmount();

        if (currentAmount + amount > maxCapacity)
        {
            Debug.Log("Inventory is full!!!");
            return false;
        }

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

        OnInventoryChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public bool RemoveItem(ItemSO item, int amount)
    {
        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            if (inventoryItemList[i].item != item)
                continue;

            if (inventoryItemList[i].amount < amount)
                return false;

            inventoryItemList[i].AddAmount(-amount);

            if (inventoryItemList[i].amount <= 0)
                inventoryItemList.RemoveAt(i);

            OnInventoryChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        return false;
    }

    public int GetTotalAmount()
    {
        int total = 0;

        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            total += inventoryItemList[i].amount;
        }

        return total;
    }

    public void TransferTo(GlobalInventorySO targetInventory)
    {
        if (inventoryItemList.Count == 0)
            return;

        for (int i = 0; i < inventoryItemList.Count; i++)
            targetInventory.AddItem(inventoryItemList[i].item, inventoryItemList[i].amount);

        inventoryItemList.Clear();
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ForceRefresh() => OnInventoryChanged?.Invoke(this, EventArgs.Empty);

    public void Reset() => inventoryItemList.Clear();
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
