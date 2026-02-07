using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    [SerializeField] private LayerMask unitLayer;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleAttack();
        }
    }

    private void HandleAttack()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, unitLayer))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                unit.MoveToInteract(interactable, hit.point);
                return;
            }
        }
    }
}
