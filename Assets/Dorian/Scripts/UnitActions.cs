using JetBrains.Annotations;
using UnityEngine;

public class UnitActions : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private LayerMask oreLayerMask;
    [SerializeField] private LayerMask unitLayerMask;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if(HandleMining())
                return;
            if (HandleInteraction())
                return;

            HandleAttack();
        }
    }

    private bool HandleInteraction()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null) return false;

        var hitCheck = GetRaycastHit(interactableLayerMask);

        if (!hitCheck.HasValue)
            return false;
        
        RaycastHit hit = hitCheck.Value;
        
        if (hit.transform.TryGetComponent(out IInteractable interactable))
        {
            unit.MoveToInteract(interactable, hit.point);
            return true;
        }

        return false;
    }
    
    private void HandleAttack()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null) return;

        var hitCheck = GetRaycastHit(unitLayerMask);

        if (!hitCheck.HasValue)
            return;
        
        RaycastHit hit = hitCheck.Value;
        
        if (hit.transform.TryGetComponent(out EnemyUnit enemy))
        {
            unit.MoveToAttack(enemy, hit.point);
        }
    }

    private bool HandleMining()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null) return false;

        var hitCheck = GetRaycastHit(oreLayerMask);

        if (!hitCheck.HasValue)
            return false;

        RaycastHit hit = hitCheck.Value;

        if (hit.transform.TryGetComponent(out IMineable mineable))
        {
            unit.MoveToMine(mineable, hit.point);
            return true;
        }

        return false;
    }

    private RaycastHit? GetRaycastHit(LayerMask mask)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mask))
        {
            return hit;
        }

        return null;
    }
}

