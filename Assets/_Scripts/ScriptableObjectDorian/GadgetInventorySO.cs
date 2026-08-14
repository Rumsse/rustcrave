using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetInventorySO", menuName = "Scriptable Objects/GadgetInventorySO")]
public class GadgetInventorySO : InventorySO, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    void OnEnable() => OnInventoryChanged += NotifyChanges;

    void OnDisable() => OnInventoryChanged -= NotifyChanges;

    void NotifyChanges(object sender, EventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

    public List<GadgetSO> GetAllAvailableGadgets()
    {
        return inventoryItemList
            .Where(slot => slot.item is GadgetSO)
            .Select(slot => slot.item as GadgetSO)
            .ToList();
    }

    public int GetGadgetAmount(GadgetSO gadget)
    {
        var slot = inventoryItemList.FirstOrDefault(s => s.item == gadget);

        if (slot == null)
            return 0;

        return slot.amount;
    }
}