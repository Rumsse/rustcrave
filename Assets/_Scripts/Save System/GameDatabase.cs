using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Save System/GameDatabase")]
public class GameDatabase : ScriptableObject
{
    [SerializeField] private List<UnitData> allUnits = new();
    [SerializeField] private List<ItemData> allItems = new();

    public UnitData GetUnit(string unitName) => allUnits.FirstOrDefault(u => u.name == unitName);

    public ItemData GetItem(string itemName) => allItems.FirstOrDefault(i => i.name == itemName);

    public GadgetData GetGadget(string gadgetName) => GetItem(gadgetName) as GadgetData;
}