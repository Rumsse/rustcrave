using UnityEngine;

public class UnitInteract : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayerMask;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, interactableLayerMask))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                unit.MoveToInteract(interactable, hit.point);
                return;
            }
        }
    }
}

