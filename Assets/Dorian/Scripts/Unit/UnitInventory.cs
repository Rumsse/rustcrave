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
        if (statsManager != null)
        {
            int newCapacity = statsManager.CarryCapacity;
            if (inventorySO.maxCapacity != newCapacity)
            {
                inventorySO.maxCapacity = newCapacity;
                inventorySO.ForceRefresh();
            }
        }
        else
        {
            inventorySO.maxCapacity = unitSO.carryCapacity;
        }
    }

    private void OnApplicationQuit()
    {
        if (inventorySO != null && inventorySO.inventoryItemList != null)
            inventorySO.inventoryItemList.Clear();
    }
}