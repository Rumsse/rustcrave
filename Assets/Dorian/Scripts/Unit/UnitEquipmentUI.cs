using System.Collections.Generic;
using UnityEngine;

public class UnitEquipmentUI : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;

    private readonly List<GadgetSlotUI> spawnedSlots = new();

    private void Awake()
    {
        foreach (Transform child in slotsContainer)
        {
            if (child.TryGetComponent<GadgetSlotUI>(out var slot))
                spawnedSlots.Add(slot);
        }
    }

    public void RefreshSlots(Unit unit)
    {
        if (unit == null || !unit.TryGetComponent<UnitEquipment>(out var equipment))
        {
            foreach (var slot in spawnedSlots)
                slot.gameObject.SetActive(false);

            return;
        }

        List<GadgetSO> gadgets = equipment.GetEquippedGadgets();
        int maxSlots = equipment.GetMaxSlots();

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (i >= maxSlots)
            {
                spawnedSlots[i].gameObject.SetActive(false);
                continue;
            }

            spawnedSlots[i].gameObject.SetActive(true);

            if (i < gadgets.Count)
                spawnedSlots[i].SetItem(gadgets[i]);
            else
                spawnedSlots[i].ClearSlot();
        }
    }
}