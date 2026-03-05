using UnityEngine;

public class UnitInventory : MonoBehaviour
{
    [SerializeField] private InventorySO inventorySO;

    private void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<Item>();

        if (item)
        {
            inventorySO.AddItem(item.itemSO, 1);
            Destroy(other.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        inventorySO.inventoryItemList.Clear();
    }
}
