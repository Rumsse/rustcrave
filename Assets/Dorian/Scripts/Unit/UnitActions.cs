using UnityEngine;

public class UnitActions : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private LayerMask oreLayerMask;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask mouseWorldLayerMask;

    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private CommandVisualizer commandVisualizer;
    [SerializeField] private float indicatorYOffset;

    private bool isDraggingCommand;

    private void Update()
    {
        HandleAbilityInput();
        HandleCommandInput();
    }

    private void HandleAbilityInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();

            if (unit != null && unit.enabled)
            {
                if (unit.TryGetComponent<ActiveAbility>(out var ability))
                {
                    ability.TryExecute();
                }
            }
        }
    }

    private void HandleCommandInput()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        
        if (unit != null && !unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Spider)
            {
                return;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (unit != null && unit.enabled)
            {
                isDraggingCommand = true;
                if (commandVisualizer != null)
                {
                    commandVisualizer.StartVisuals(unit);
                }
            }
        }

        if (Input.GetMouseButton(1) && isDraggingCommand && unit != null)
        {
            if (commandVisualizer != null)
            {
                commandVisualizer.UpdateVisuals(unit);
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (isDraggingCommand)
            {
                isDraggingCommand = false;

                if (commandVisualizer != null)
                {
                    commandVisualizer.StopVisuals();
                }

                unit.PlayCommandSound();
                ExecuteCommand();
            }
        }
    }

    private void ExecuteCommand()
    {
        if (HandleMining())
        {
            PlayMCCommandAnimation();
            return;
        }
        if (HandleInteraction())
        {
            PlayMCCommandAnimation();
            return;
        }
        if (HandleAttack())
        {
            PlayMCCommandAnimation();
            return;
        }

        if (HandleMovement())
        {
            PlayMCCommandAnimation();
        }
    }

    private void PlayMCCommandAnimation()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit != null && !unit.IsMainCharacter)
        {
            Unit.MainCharacter?.PlayCommandAnimation();
        }
    }

    private bool HandleInteraction()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null || !unit.enabled) return false;

        var hitCheck = GetRaycastHit(interactableLayerMask);

        if (!hitCheck.HasValue)
            return false;

        RaycastHit hit = hitCheck.Value;

        if (hit.transform.TryGetComponent(out IInteractable interactable))
        {
            unit.MoveToInteract(interactable, hit.point);
            ShowMoveIndicator(hit.point);
            return true;
        }

        return false;
    }

    private bool HandleAttack()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null || !unit.enabled) return false;

        if (unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
            {
                return false;
            }
        }

        var hitCheck = GetRaycastHit(unitLayerMask);

        if (!hitCheck.HasValue)
            return false;

        RaycastHit hit = hitCheck.Value;

        if (hit.transform.TryGetComponent(out EnemyUnit enemy))
        {
            unit.MoveToAttack(enemy, hit.point);
            ShowMoveIndicator(hit.point);
            return true;
        }

        return false;
    }

    private bool HandleMining()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null || !unit.enabled) return false;

        if (unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
            {
                return false;
            }
        }

        var hitCheck = GetRaycastHit(oreLayerMask);

        if (!hitCheck.HasValue)
            return false;

        RaycastHit hit = hitCheck.Value;

        if (hit.transform.TryGetComponent(out IMineable mineable))
        {
            unit.MoveToMine(mineable, hit.point);
            ShowMoveIndicator(hit.point);
            return true;
        }

        return false;
    }

    private bool HandleMovement()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null || !unit.enabled) return false;

        var hitCheck = GetRaycastHit(mouseWorldLayerMask);

        if (hitCheck.HasValue)
        {
            RaycastHit hit = hitCheck.Value;
            unit.HandleMovement(hit.point);
            ShowMoveIndicator(hit.point);
            return true;
        }
        return false;
    }

    private void ShowMoveIndicator(Vector3 position)
    {
        if (moveIndicatorPrefab != null)
        {
            Instantiate(moveIndicatorPrefab, position + new Vector3(0f, indicatorYOffset, 0f), Quaternion.identity);
        }
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