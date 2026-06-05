using System;
using UnityEngine;

public class UnitSelectedVisual : MonoBehaviour
{
    [SerializeField] private Unit _unit;
    [SerializeField] private GameObject _visualGameObject;

    private void Start()
    {
        UnitSelectionSystem.Instance.OnSelectedUnitChanged += UnitSelectionSystem_OnSelectedUnitChanged;
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.OnSelectedUnitChanged -= UnitSelectionSystem_OnSelectedUnitChanged;
    }

    private void UnitSelectionSystem_OnSelectedUnitChanged(object sender, EventArgs e) => UpdateVisual();

    private void UpdateVisual()
    {
        if (_visualGameObject == null)
            return;

        bool isSelected = UnitSelectionSystem.Instance.GetSelectedUnit() == _unit;
        _visualGameObject.SetActive(isSelected);

        if (isSelected && _unit != null)
        {
            if (_unit.TryGetComponent(out HealthManager healthManager))
                healthManager.UpdateHealthVisuals();
        }
    }
}