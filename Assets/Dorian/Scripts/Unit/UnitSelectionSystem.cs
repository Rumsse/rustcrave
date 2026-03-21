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

        if (TryHandleUnitSelection())
            return;

        if (!MouseWorld.TryGetPosition(out Vector3 position))
            return;

        if (selectedUnit == null)
            return;

        if (!selectedUnit.IsMainCharacter && MCFormController.Instance != null && MCFormController.Instance.GetCurrentForm() == CharacterForm.Spider)
            return;

        selectedUnit.HandleMovement(position);
        ShowMoveIndicator(position);

        if (!selectedUnit.IsMainCharacter)
            Unit.MainCharacter?.PlayCommandAnimation();
    }

    private bool TryHandleUnitSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out Unit unit))
            {
                SetSelectedUnit(unit);
                return true;
            }
        }
        return false;
    }

    private void ShowMoveIndicator(Vector3 position)
    {
        if (moveIndicatorPrefab != null)
        {
            Instantiate(moveIndicatorPrefab, position + new Vector3(0, 0.05f, 0), Quaternion.identity);
        }
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);

        if (unit != null && !unit.IsMainCharacter)
        {
            Unit.MainCharacter?.PlayCommandAnimation();
        }
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }
}