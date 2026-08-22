using System;
using UnityEngine;

public class UnitSelectedVisual : MonoBehaviour
{
    [SerializeField] private Unit _unit;
    [SerializeField] private GameObject _visualGameObject;

    private void Start()
    {
        UnitSelectionSystem.Instance.OnSelectedUnitsChanged += UnitSelectionSystem_OnSelectedUnitsChanged;
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.OnSelectedUnitsChanged -= UnitSelectionSystem_OnSelectedUnitsChanged;
    }

    private void UnitSelectionSystem_OnSelectedUnitsChanged(object sender, EventArgs e) => UpdateVisual();

    private void UpdateVisual()
    {
        if (_visualGameObject == null)
            return;

        bool isSelected = UnitSelectionSystem.Instance.GetSelectedUnits().Contains(_unit);
        _visualGameObject.SetActive(isSelected);

        if (!isSelected || _unit == null)
            return;

        if (_unit.TryGetComponent(out HealthManager healthManager))
            healthManager.UpdateHealthVisuals();
    }
}