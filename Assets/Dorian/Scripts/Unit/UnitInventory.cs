using UnityEngine;

public class UnitInventory : MonoBehaviour
{
    public InventorySO InventorySO => inventorySO;

    [SerializeField] protected InventorySO inventorySO;
    [SerializeField] private UnitSO unitSO;

    private StatsManager statsManager;

    private void Awake()
    {
        inventorySO = Instantiate(inventorySO);
        statsManager = GetComponent<StatsManager>();
    }

    private void Start()
    {
        UpdateCapacity();
    }

    public void UpdateCapacity()
    {
        if (statsManager == null)
        {
            inventorySO.maxCapacity = unitSO.carryCapacity;
            return;
        }

        int newCapacity = statsManager.CarryCapacity;

        if (inventorySO.maxCapacity == newCapacity)
            return;

        inventorySO.maxCapacity = newCapacity;
        inventorySO.ForceRefresh();
    }

    private void OnApplicationQuit()
    {
        if (inventorySO != null && inventorySO.inventoryItemList != null)
            inventorySO.inventoryItemList.Clear();
    }
}