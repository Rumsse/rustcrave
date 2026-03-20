using UnityEngine;
using System.Collections.Generic;

public class UnitEquipmentUI : MonoBehaviour
{
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject gadgetSlotPrefab;

    public void RefreshSlots(Unit unit)
    {
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }

        if (unit.TryGetComponent<UnitEquipment>(out var equipment))
        {
            List<GadgetSO> gadgets = equipment.GetEquippedGadgets();
            int maxSlots = equipment.GetMaxSlots();

            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotGO = Instantiate(gadgetSlotPrefab, slotsContainer);

                if (slotGO.TryGetComponent<GadgetSlotUI>(out var slotUI))
                {
                    if (i < gadgets.Count)
                    {
                        slotUI.SetItem(gadgets[i]);
                    }
                    else
                    {
                        slotUI.ClearSlot();
                    }
                }
            }
        }
    }
}