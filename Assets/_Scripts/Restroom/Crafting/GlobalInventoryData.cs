using System;
using System.ComponentModel;
using System.Linq;
using Unity.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalInventoryData", menuName = "Swarm/Inventory/GlobalInventoryData")]
public class GlobalInventoryData : InventoryData, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    [CreateProperty] public int Sparklite => GetItemAmount("sparklite");
    [CreateProperty] public int Scraponite => GetItemAmount("scraponite");
    [CreateProperty] public int Pulsite => GetItemAmount("pulsite");

    void OnEnable() => OnInventoryChanged += NotifyChanges;

    void OnDisable() => OnInventoryChanged -= NotifyChanges;

    void NotifyChanges(object sender, EventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

    int GetItemAmount(string id)
    {
        var slot = inventoryItemList.FirstOrDefault(s => s.item is OreData ore && ore.oreName.ToLower() == id);

        if (slot == null)
            return 0;

        return slot.amount;
    }

}