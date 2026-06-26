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

    // Logic to steal random item from inventory, used in steal attack
    public ItemSO StealRandomItem()
    {
        if (inventoryItemList.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, inventoryItemList.Count);
        ItemSO stolenItem = inventoryItemList[randomIndex].item;

        RemoveItem(stolenItem, 1);
        return stolenItem;
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
        Debug.Log($"[InventorySO] TransferTo called. Items to transfer: {inventoryItemList.Count}");

        if (inventoryItemList.Count == 0)
            return;

        for (int i = 0; i < inventoryItemList.Count; i++)
            targetInventory.AddItem(inventoryItemList[i].item, inventoryItemList[i].amount);

        inventoryItemList.Clear();
        OnInventoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ForceRefresh() => OnInventoryChanged?.Invoke(this, EventArgs.Empty);

    public void Reset() => inventoryItemList.Clear();


    public InventorySaveData GetSaveData()
    {
        var data = new InventorySaveData();

        foreach (var slot in inventoryItemList)
        {
            data.slots.Add(new SlotSaveData
            {
                itemName = slot.item.name,
                amount = slot.amount
            });
        }

        return data;
    }

    public void LoadFromSave(InventorySaveData data, GameDatabase db)
    {
        inventoryItemList.Clear();

        foreach (var slotData in data.slots)
        {
            var item = db.GetItem(slotData.itemName);

            if (item == null)
            {
                Debug.LogError($"Cannot load item. {slotData.itemName} is missing in GameDatabaseSO!");
                continue;
            }

            inventoryItemList.Add(new InventorySlot(item, slotData.amount));
        }

        ForceRefresh();
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
