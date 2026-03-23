using System;
using UnityEngine;

public class UnitSelectionSystem : MonoBehaviour
{
    public static UnitSelectionSystem Instance;

    public event EventHandler OnSelectedUnitChanged;

    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask mouseWorldLayerMask;
    [SerializeField] private GameObject moveIndicatorPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        TryHandleUnitSelection();
    }

    private bool TryHandleUnitSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            return false;

        if (!raycastHit.transform.TryGetComponent(out Unit unit))
            return false;

        SetSelectedUnit(unit);
        return true;
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;

        if (unit != null)
            unit.PlaySelectSound();

        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);

        if (unit == null)
            return;

        if (unit.IsMainCharacter)
            return;

        Unit.MainCharacter?.PlayCommandAnimation();
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }
}