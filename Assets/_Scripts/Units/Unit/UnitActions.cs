using System.Collections;
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

        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();

        if (!unit || !commandVisualizer)
            return;

        commandVisualizer.UpdateVisuals(unit);
        TryPlayDragSound();
    }

    #endregion

    #region Input Handlers

    private void OnAbilityPerformed(InputAction.CallbackContext context)
    {
        if (Time.timeScale == PAUSED_TIME_SCALE)
            return;

        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();

        if (unit && unit.enabled && unit.TryGetComponent<ActiveAbility>(out var ability))
            ability.TryExecute();
    }

    private void OnCommandStarted(InputAction.CallbackContext context)
    {
        if (Time.timeScale == PAUSED_TIME_SCALE)
            return;

        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();

        if (!unit)
        {
            ResetDragState();
            return;
        }

        if (!unit.IsMainCharacter && MCFormController.Instance && MCFormController.Instance.GetCurrentForm() == CharacterForm.Spider)
        {
            ResetDragState();
            return;
        }

        if (!unit.enabled)
            return;

        isDraggingCommand = true;

        if (commandVisualizer)
            commandVisualizer.StartVisuals(unit);
    }

    private void OnCommandCanceled(InputAction.CallbackContext context)
    {
        if (!isDraggingCommand)
            return;

        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        ResetDragState();

        if (unit && CanUnitReceiveCommands(unit))
        {
            TryPlayExecutionSound(unit);
            ExecuteCommand(unit);
        }
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

    private void ExecuteCommand(Unit unit)
    {
        if (HandleMining(unit))
        {
            PlayMCCommandAnimation(unit);
            return;
        }

        if (HandleInteraction(unit))
        {
            PlayMCCommandAnimation(unit);
            return;
        }

        if (HandleAttack(unit))
        {
            PlayMCCommandAnimation(unit);
            return;
        }

        if (HandleMovement(unit))
            PlayMCCommandAnimation(unit);
    }

    private bool HandleInteraction(Unit unit)
    {
        if (TryGetTargetUnderPointer<IInteractable>(interactableLayerMask, out var interactable, out Vector3 hitPoint, out Transform targetTransform))
        {
            unit.MoveToInteract(interactable, hitPoint);
            ShowActionFeedback(interactFeedbackPrefab, targetTransform);
            return true;
        }

        return false;
    }

    private bool HandleAttack(Unit unit)
    {
        if (unit.IsMainCharacter && MCFormController.Instance && MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
            return false;

        if (TryGetTargetUnderPointer<EnemyUnit>(unitLayerMask, out var enemy, out Vector3 hitPoint, out Transform targetTransform))
        {
            unit.MoveToAttack(enemy, hitPoint);
            ShowActionFeedback(attackFeedbackPrefab, targetTransform);
            return true;
        }

        return false;
    }

    private bool HandleMining(Unit unit)
    {
        if (unit.IsMainCharacter && MCFormController.Instance && MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
            return false;

        if (TryGetTargetUnderPointer<IMineable>(oreLayerMask, out var mineable, out Vector3 hitPoint, out Transform targetTransform))
        {
            unit.MoveToMine(mineable, hitPoint);
            ShowActionFeedback(mineFeedbackPrefab, targetTransform);
            return true;
        }

        return false;
    }

    private bool HandleMovement(Unit unit)
    {
        Vector2 pointerPos = input.Gameplay.PointerPosition.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(pointerPos);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mouseWorldLayerMask))
        {
            unit.HandleMovement(hit.point);
            ShowMoveIndicator(hit.point);
            return true;
        }

        return false;
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