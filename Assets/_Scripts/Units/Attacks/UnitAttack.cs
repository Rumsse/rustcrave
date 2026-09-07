using System.Collections.Generic;
using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    [SerializeField] private LayerMask unitLayer;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleAttack();
    }

    private void HandleAttack()
    {
        List<Unit> selectedUnits = UnitSelectionSystem.Instance.GetSelectedUnits();

        if (selectedUnits.Count == 0)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, unitLayer))
            return;

        if (!hit.transform.TryGetComponent(out IInteractable interactable))
            return;

        foreach (Unit unit in selectedUnits)
            unit.MoveToInteract(interactable, hit.point);
    }
}