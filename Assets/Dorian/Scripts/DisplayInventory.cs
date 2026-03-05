using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayInventory : MonoBehaviour
{
    [SerializeField] private InventorySO inventorySO;

    [SerializeField] private int xStart;
    [SerializeField] private int yStart;

    [SerializeField] private int xSpaceBetweenItems;
    [SerializeField] private int ySpaceBetweenItems;
    [SerializeField] private int numberOfColumn;

    Dictionary<InventorySlot,GameObject> itemsDisplayed = new Dictionary<InventorySlot,GameObject>();

    private void Start()
    {
        CreateDisplay();
    }


    private void Update()
    {
        UpdateDisplay();
    }

    private void CreateDisplay()
    {
        for (int i = 0; i < inventorySO.inventoryItemList.Count; i++)
        {
            var obj = Instantiate(inventorySO.inventoryItemList[i].item.prefab, Vector3.zero, Quaternion.identity, 
                transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = inventorySO.inventoryItemList[i].amount.ToString("n0");
        }
    }

    private Vector3 GetPosition(int i)
    {
        return new Vector3(xStart + (xSpaceBetweenItems * (i % numberOfColumn)),yStart + (-ySpaceBetweenItems * (i / numberOfColumn)), 0f);
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < inventorySO.inventoryItemList.Count; i++)
        {
            if (itemsDisplayed.ContainsKey(inventorySO.inventoryItemList[i]))
            {
                itemsDisplayed[inventorySO.inventoryItemList[i]].GetComponentInChildren<TextMeshProUGUI>().text = 
                    inventorySO.inventoryItemList[i].amount.ToString("n0");
            }
            else
            {
                var obj = Instantiate(inventorySO.inventoryItemList[i].item.prefab, Vector3.zero, Quaternion.identity,
                transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = inventorySO.inventoryItemList[i].amount.ToString("n0");
                itemsDisplayed.Add(inventorySO.inventoryItemList[i], obj);
            }
        }
    }
}
