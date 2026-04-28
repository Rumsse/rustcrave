using UnityEngine;

public class UnitActions : MonoBehaviour
{
    [Header("Layer Masks")]
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private LayerMask oreLayerMask;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask mouseWorldLayerMask;
    [SerializeField] private StatsManager statsManager;

    [Header("Visual Feedback Prefabs")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private GameObject attackFeedbackPrefab;
    [SerializeField] private GameObject mineFeedbackPrefab;
    [SerializeField] private GameObject interactFeedbackPrefab;
    [SerializeField] private float indicatorYOffset;

    [Header("Visualizers")]
    [SerializeField] private CommandVisualizer commandVisualizer;

    private bool isDraggingCommand;

    private void Update()
    {
        if (Time.timeScale == 0f) return;

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

        if (unit != null && unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.IsTransitioning) return;
        }

        if (unit != null && !unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Spider) return;
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

        if (TryGetTargetUnderMouse<IInteractable>(interactableLayerMask, out var interactable, out Vector3 hitPoint, out Transform targetTransform))
        {
            unit.MoveToInteract(interactable, hitPoint);
            ShowActionFeedback(interactFeedbackPrefab, targetTransform);
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
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor) return false;
        }

        if (TryGetTargetUnderMouse<EnemyUnit>(unitLayerMask, out var enemy, out Vector3 hitPoint, out Transform targetTransform))
        {
            unit.MoveToAttack(enemy, hitPoint);
            ShowActionFeedback(attackFeedbackPrefab, targetTransform);
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
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor) return false;
        }

        if (TryGetTargetUnderMouse<IMineable>(oreLayerMask, out var mineable, out Vector3 hitPoint, out Transform targetTransform))
        {
            unit.MoveToMine(mineable, hitPoint);
            ShowActionFeedback(mineFeedbackPrefab, targetTransform);
            return true;
        }

        return false;
    }

    private bool HandleMovement()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null || !unit.enabled) return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mouseWorldLayerMask))
        {
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

    private void ShowActionFeedback(GameObject prefab, Transform target)
    {
        if (prefab != null && target != null)
        {
            Instantiate(prefab, target.position, Quaternion.identity, target);
        }
    }

    private bool TryGetTargetUnderMouse<T>(LayerMask mask, out T targetComponent, out Vector3 point, out Transform targetTransform) where T : class
    {
        targetComponent = null;
        point = Vector3.zero;
        targetTransform = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, mask);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.transform.TryGetComponent(out T component))
            {
                targetComponent = component;
                point = hit.point;
                targetTransform = hit.transform;
                return true;
            }

            T parentComponent = hit.transform.GetComponentInParent<T>();
            if (parentComponent != null)
            {
                targetComponent = parentComponent;
                point = hit.point;
                targetTransform = hit.transform;
                return true;
            }
        }
        return false;
    }
}