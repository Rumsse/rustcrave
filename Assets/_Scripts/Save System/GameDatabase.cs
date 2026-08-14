using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Save System/GameDatabase")]
public class GameDatabase : ScriptableObject
{
    [SerializeField] private List<UnitData> allUnits = new();
    [SerializeField] private List<ItemSO> allItems = new();

    public UnitData GetUnit(string unitName) => allUnits.FirstOrDefault(u => u.name == unitName);

    public ItemSO GetItem(string itemName) => allItems.FirstOrDefault(i => i.name == itemName);

    public GadgetSO GetGadget(string gadgetName) => GetItem(gadgetName) as GadgetSO;
}