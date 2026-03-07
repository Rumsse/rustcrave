using UnityEngine;

public class UnitInventoryUI : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private GameObject unitInventory;


    private void Start()
    {
        UnitSelectionSystem.Instance.OnSelectedUnitChanged += UnitSelectionSystem_OnSelectedUnitChanged;
        UpdateVisual();
    }

    private void UnitSelectionSystem_OnSelectedUnitChanged(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (UnitSelectionSystem.Instance.GetSelectedUnit() == unit)
        {
            unitInventory.SetActive(true);
        }
        else
        {
            unitInventory.SetActive(false);
        }
    }

}
