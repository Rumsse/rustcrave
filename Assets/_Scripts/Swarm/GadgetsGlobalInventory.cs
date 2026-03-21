using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetsGlobalInventory", menuName = "Restroom/Swarm/GadgetsGlobalInventory")]
public class GadgetsGlobalInventory : ScriptableObject
{
    public List<GadgetSO> unlockedGadgets = new();

    public void AddGadget(GadgetSO gadget)
    {
        if (gadget == null)
            return;

        if (unlockedGadgets.Contains(gadget))
            return;

        unlockedGadgets.Add(gadget);
    }

    public void Reset() => unlockedGadgets.Clear();
}