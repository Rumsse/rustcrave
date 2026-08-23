using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class UnitActions : MonoBehaviour
{
    #region Configuration

    [Header("Layer Masks")]
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private LayerMask oreLayerMask;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask mouseWorldLayerMask;

    [Header("References")]
    [SerializeField] private StatsManager statsManager;
    [SerializeField] private CommandVisualizer commandVisualizer;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private GameObject attackFeedbackPrefab;
    [SerializeField] private GameObject mineFeedbackPrefab;
    [SerializeField] private GameObject interactFeedbackPrefab;
    [SerializeField] private float indicatorYOffset;

    [Header("Audio & Timings")]
    [SerializeField] private EventReference orderSound;
    [SerializeField] private float dragSoundCooldown = 0.3f;
    [SerializeField] private float executionSoundCooldown = 0.5f;
    [SerializeField] private float executionSoundDelay = 0.2f;
    [SerializeField] private float unitSoundDelay = 0.3f;
    [SerializeField] private float commandAnimationCooldown = 0.5f;

    [Header("Formation")]
    [SerializeField] private float formationSpacing = 1.5f;

    #endregion

    #region State

    private PlayerControls input;

    private const float PAUSED_TIME_SCALE = 0f;

    private bool isDraggingCommand;
    private float lastDragSoundTime;
    private float lastExecutionSoundTime;
    private float lastCommandAnimationTime;

    #endregion

    #region Unity Lifecycle

    private void Awake() => input = new PlayerControls();

    private void OnEnable()
    {
        input.Enable();
        input.Gameplay.Command.started += OnCommandStarted;
        input.Gameplay.Command.canceled += OnCommandCanceled;
        input.Gameplay.Ability.performed += OnAbilityPerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Gameplay.Command.started -= OnCommandStarted;
        input.Gameplay.Command.canceled -= OnCommandCanceled;
        input.Gameplay.Ability.performed -= OnAbilityPerformed;
    }

    private void Update()
    {
        if (!isDraggingCommand || Time.timeScale == PAUSED_TIME_SCALE)
            return;

        Unit primaryUnit = UnitSelectionSystem.Instance.GetPrimarySelectedUnit();

        if (!primaryUnit || !commandVisualizer)
            return;

        commandVisualizer.UpdateVisuals(primaryUnit);
        TryPlayDragSound();
    }

    #endregion

    #region Input Handlers

    private void OnAbilityPerformed(InputAction.CallbackContext context)
    {
        if (Time.timeScale == PAUSED_TIME_SCALE)
            return;

        List<Unit> units = UnitSelectionSystem.Instance.GetSelectedUnits();

        foreach (Unit unit in units)
        {
            if (unit && unit.enabled && unit.TryGetComponent<ActiveAbility>(out var ability))
                ability.TryExecute();
        }
    }

    private void OnCommandStarted(InputAction.CallbackContext context)
    {
        if (Time.timeScale == PAUSED_TIME_SCALE)
            return;

        List<Unit> units = UnitSelectionSystem.Instance.GetSelectedUnits();

        if (units.Count == 0)
        {
            ResetDragState();
            return;
        }

        Unit primaryUnit = units[0];

        if (!primaryUnit.IsMainCharacter && MCFormController.Instance && MCFormController.Instance.GetCurrentForm() == CharacterForm.Spider)
        {
            ResetDragState();
            return;
        }

        isDraggingCommand = true;

        if (commandVisualizer)
            commandVisualizer.StartVisuals(primaryUnit);
    }

    private void OnCommandCanceled(InputAction.CallbackContext context)
    {
        if (!isDraggingCommand)
            return;

        List<Unit> units = UnitSelectionSystem.Instance.GetSelectedUnits();
        ResetDragState();

        if (units.Count == 0)
            return;

        TryPlayExecutionSound(units[0]);
        ExecuteCommand(units);
    }

    #endregion

    #region Command Logic

    private void ResetDragState()
    {
        if (!isDraggingCommand)
            return;

        isDraggingCommand = false;

        if (commandVisualizer)
            commandVisualizer.StopVisuals();
    }

    private bool CanUnitReceiveCommands(Unit unit)
    {
        if (!unit.enabled)
            return false;

        if (unit.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent) && !agent.enabled)
            return false;

        return true;
    }

    private void ExecuteCommand(List<Unit> units)
    {
        if (HandleMining(units)) return;
        if (HandleInteraction(units)) return;
        if (HandleAttack(units)) return;

        HandleMovementCommand(units);
    }

    private bool HandleInteraction(List<Unit> units)
    {
        if (!TryGetTargetUnderPointer<IInteractable>(interactableLayerMask, out var interactable, out Vector3 hitPoint, out Transform targetTransform))
            return false;

        List<Vector3> formationPositions = CalculateFormationPositions(hitPoint, units.Count);

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i];

            if (!CanUnitReceiveCommands(unit))
                continue;

            unit.MoveToInteract(interactable, hitPoint);
        }

        ShowActionFeedback(interactFeedbackPrefab, targetTransform);
        PlayMCCommandAnimation(units[0]);
        return true;
    }

    private bool HandleAttack(List<Unit> units)
    {
        if (!TryGetTargetUnderPointer<EnemyUnit>(unitLayerMask, out var enemy, out Vector3 hitPoint, out Transform targetTransform))
            return false;

        List<Vector3> formationPositions = CalculateFormationPositions(hitPoint, units.Count);

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i];

            if (!CanUnitReceiveCommands(unit))
                continue;

            if (unit.IsMainCharacter && MCFormController.Instance && MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
            {
                unit.HandleMovement(formationPositions[i]);
                continue;
            }

            unit.MoveToAttack(enemy, hitPoint);
        }

        ShowActionFeedback(attackFeedbackPrefab, targetTransform);
        PlayMCCommandAnimation(units[0]);
        return true;
    }

    private bool HandleMining(List<Unit> units)
    {
        if (!TryGetTargetUnderPointer<IMineable>(oreLayerMask, out var mineable, out Vector3 hitPoint, out Transform targetTransform))
            return false;

        List<Vector3> formationPositions = CalculateFormationPositions(hitPoint, units.Count);

        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i];

            if (!CanUnitReceiveCommands(unit))
                continue;

            if (unit.IsMainCharacter && MCFormController.Instance && MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
            {
                unit.HandleMovement(formationPositions[i]);
                continue;
            }

            unit.MoveToMine(mineable, hitPoint);
        }

        ShowActionFeedback(mineFeedbackPrefab, targetTransform);
        PlayMCCommandAnimation(units[0]);
        return true;
    }

    private bool HandleMovementCommand(List<Unit> units)
    {
        Vector2 pointerPos = input.Gameplay.PointerPosition.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(pointerPos);

        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mouseWorldLayerMask))
            return false;

        List<Vector3> formationPositions = CalculateFormationPositions(hit.point, units.Count);

        for (int i = 0; i < units.Count; i++)
        {
            if (CanUnitReceiveCommands(units[i]))
                units[i].HandleMovement(formationPositions[i]);
        }

        ShowMoveIndicator(hit.point);
        PlayMCCommandAnimation(units[0]);
        return true;
    }

    private List<Vector3> CalculateFormationPositions(Vector3 targetPosition, int unitCount)
    {
        List<Vector3> positions = new();

        if (unitCount == 1)
        {
            positions.Add(targetPosition);
            return positions;
        }

        int rows = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
        int cols = Mathf.CeilToInt((float)unitCount / rows);

        float startX = -(cols - 1) * formationSpacing / 2f;
        float startZ = -(rows - 1) * formationSpacing / 2f;

        for (int i = 0; i < unitCount; i++)
        {
            int row = i / cols;
            int col = i % cols;
            Vector3 pos = targetPosition + new Vector3(startX + col * formationSpacing, 0f, startZ + row * formationSpacing);
            positions.Add(pos);
        }

        return positions;
    }

    private bool TryGetTargetUnderPointer<T>(LayerMask mask, out T targetComponent, out Vector3 point, out Transform targetTransform) where T : class
    {
        targetComponent = null;
        point = Vector3.zero;
        targetTransform = null;

        Vector2 pointerPos = input.Gameplay.PointerPosition.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(pointerPos);
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

    #endregion

    #region Audio & Visuals

    private void TryPlayDragSound()
    {
        if (Time.time - lastDragSoundTime < dragSoundCooldown)
            return;

        AudioManager.PlayOneShot(orderSound);
        lastDragSoundTime = Time.time;
    }

    private void TryPlayExecutionSound(Unit unit)
    {
        if (Time.time - lastExecutionSoundTime < executionSoundCooldown)
            return;

        lastExecutionSoundTime = Time.time;
        StartCoroutine(DelayCommandSoundRoutine(unit));
    }

    private IEnumerator DelayCommandSoundRoutine(Unit unit)
    {
        yield return new WaitForSeconds(executionSoundDelay);

        if (statsManager)
            AudioManager.PlayOneShot(statsManager.Sounds.commandSound);

        if (unit && unit.enabled && !unit.IsMainCharacter)
        {
            yield return new WaitForSeconds(unitSoundDelay);

            if (unit && unit.enabled)
                unit.PlayCommandSound();
        }
    }

    private void PlayMCCommandAnimation(Unit unit)
    {
        if (Time.time - lastCommandAnimationTime < commandAnimationCooldown)
            return;

        if (unit && !unit.IsMainCharacter)
        {
            Unit.MainCharacter?.PlayCommandAnimation();
            lastCommandAnimationTime = Time.time;
        }
    }

    private void ShowMoveIndicator(Vector3 position)
    {
        if (moveIndicatorPrefab)
            Instantiate(moveIndicatorPrefab, position + new Vector3(0f, indicatorYOffset, 0f), Quaternion.identity);
    }

    private void ShowActionFeedback(GameObject prefab, Transform target)
    {
        if (prefab && target)
            Instantiate(prefab, target.position, Quaternion.identity, target);
    }

    #endregion
}