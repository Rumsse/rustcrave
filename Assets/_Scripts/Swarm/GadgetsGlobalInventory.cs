using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetsGlobalInventory", menuName = "Restroom/Swarm/GadgetsGlobalInventory")]
public class GadgetsGlobalInventory : ScriptableObject
{
    public List<GadgetData> unlockedGadgets = new();

    public void AddGadget(GadgetData gadget)
    {
        if (gadget == null)
            return;

        /*if (unlockedGadgets.Contains(gadget))
            return;*/

        unlockedGadgets.Add(gadget);
    }

    public void Reset() => unlockedGadgets.Clear();

    public GadgetsSaveData GetSaveData()
    {
        var data = new GadgetsSaveData();

        foreach (var gadget in unlockedGadgets)
            data.unlockedGadgetNames.Add(gadget.name);

        return data;
    }

    public void LoadFromSave(GadgetsSaveData data, GameDatabase db)
    {
        unlockedGadgets.Clear();

        foreach (var gadgetName in data.unlockedGadgetNames)
        {
            var gadget = db.GetGadget(gadgetName);

            if (gadget == null)
                continue;

            unlockedGadgets.Add(gadget);
        }
    }
}