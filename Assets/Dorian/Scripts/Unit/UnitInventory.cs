using UnityEngine;

public class UnitInventory : MonoBehaviour
{
    public InventorySO InventorySO => inventorySO;

    [SerializeField] protected InventorySO inventorySO;
    [SerializeField] private UnitSO unitSO;

    private void Awake()
    {
        inventorySO = Instantiate(inventorySO);
        inventorySO.maxCapacity = unitSO.carryCapacity;
    }

    private void OnApplicationQuit()
    {
        inventorySO.inventoryItemList.Clear();
    }
}