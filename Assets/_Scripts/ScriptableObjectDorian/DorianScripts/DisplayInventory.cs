using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayInventory : MonoBehaviour
{
    [SerializeField] private UnitInventory inventory;

    [SerializeField] private int xStart;
    [SerializeField] private int yStart;

    [SerializeField] private int xSpaceBetweenItems;
    [SerializeField] private int ySpaceBetweenItems;
    [SerializeField] private int numberOfColumn;

    Dictionary<InventorySlot, GameObject> itemsDisplayed = new();

    private void Start()
    {
        inventory.InventorySO.OnInventoryChanged += InventorySO_OnInventoryChanged;
        CreateDisplay();
    }

    private void InventorySO_OnInventoryChanged(object sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void CreateDisplay()
    {
        for (int i = 0; i < inventory.InventorySO.inventoryItemList.Count; i++)
        {
            var slot = inventory.InventorySO.inventoryItemList[i];

            var obj = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);

            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);

            obj.GetComponentInChildren<TextMeshProUGUI>().text =
                slot.amount.ToString("n0");

            itemsDisplayed.Add(slot, obj);
        }
    }

    private Vector3 GetPosition(int i)
    {
        return new Vector3(
            xStart + (xSpaceBetweenItems * (i % numberOfColumn)),
            yStart + (-ySpaceBetweenItems * (i / numberOfColumn)),
            0f);
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < inventory.InventorySO.inventoryItemList.Count; i++)
        {
            var slot = inventory.InventorySO.inventoryItemList[i];

            if (itemsDisplayed.ContainsKey(slot))
            {
                itemsDisplayed[slot]
                    .GetComponentInChildren<TextMeshProUGUI>()
                    .text = slot.amount.ToString("n0");
            }
            else
            {
                var obj = Instantiate(slot.item.prefab, Vector3.zero, Quaternion.identity, transform);

                obj.GetComponent<RectTransform>().localPosition = GetPosition(i);

                obj.GetComponentInChildren<TextMeshProUGUI>().text =
                    slot.amount.ToString("n0");

                itemsDisplayed.Add(slot, obj);
            }
        }
    }
}