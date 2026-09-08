using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetInventoryData", menuName = "Swarm/Inventory/GadgetInventoryData")]
public class GadgetInventoryData : InventoryData, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    void OnEnable() => OnInventoryChanged += NotifyChanges;

    void OnDisable() => OnInventoryChanged -= NotifyChanges;

    void NotifyChanges(object sender, EventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

    public List<GadgetData> GetAllAvailableGadgets()
    {
        return inventoryItemList
            .Where(slot => slot.item is GadgetData)
            .Select(slot => slot.item as GadgetData)
            .ToList();
    }

    public int GetGadgetAmount(GadgetData gadget)
    {
        var slot = inventoryItemList.FirstOrDefault(s => s.item == gadget);

        if (slot == null)
            return 0;

        return slot.amount;
    }
}