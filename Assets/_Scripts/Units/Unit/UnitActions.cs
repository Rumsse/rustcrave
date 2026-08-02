using UnityEngine;
using FMODUnity;
using System.Collections;
using UnityEngine.AI;

public class UnitActions : MonoBehaviour
{
    #region Fields & Properties

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

    [Header("Sounds")]
    [SerializeField] private EventReference orderSound;

    [Header("Sound Settings")]
    [SerializeField] private float dragSoundCooldown = 0.3f;
    [SerializeField] private float executionSoundCooldown = 0.5f;
    [SerializeField] private float executionSoundDelay = 0.2f;
    [SerializeField] private float unitSoundDelay = 0.3f;

    [Header("Animation Settings")]
    [SerializeField] private float commandAnimationCooldown = 0.5f;

    private const int RIGHT_MOUSE_BUTTON = 1;
    private const float PAUSED_TIME_SCALE = 0f;

    private bool isDraggingCommand;
    private float lastDragSoundTime;
    private float lastExecutionSoundTime;
    private float lastCommandAnimationTime;

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (Time.timeScale == PAUSED_TIME_SCALE)
            return;

        HandleAbilityInput();
        HandleCommandInput();
    }

    #endregion

    #region Input Handling

    private void HandleAbilityInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();

        if (unit != null && unit.enabled && unit.TryGetComponent<ActiveAbility>(out var ability))
            ability.TryExecute();
    }

    private void HandleCommandInput()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();

        if (unit == null)
        {
            ResetDragState();
            return;
        }

        if (!unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Spider)
            {
                ResetDragState();
                return;
            }
        }

        if (Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON))
        {
            if (unit.enabled)
            {
                isDraggingCommand = true;
                if (commandVisualizer != null)
                    commandVisualizer.StartVisuals(unit);
            }
        }

        if (Input.GetMouseButton(RIGHT_MOUSE_BUTTON) && isDraggingCommand)
        {
            if (commandVisualizer != null)
            {
                commandVisualizer.UpdateVisuals(unit);
                TryPlayDragSound();
            }
        }

        if (Input.GetMouseButtonUp(RIGHT_MOUSE_BUTTON))
        {
            if (isDraggingCommand)
            {
                ResetDragState();

                if (CanUnitReceiveCommands(unit))
                {
                    TryPlayExecutionSound(unit);
                    ExecuteCommand();
                }
            }
        }
    }

    private void ResetDragState()
    {
        if (!isDraggingCommand)
            return;

        isDraggingCommand = false;

        if (commandVisualizer != null)
            commandVisualizer.StopVisuals();
    }

    private bool CanUnitReceiveCommands(Unit unit)
    {
        if (!unit.enabled)
            return false;

        if (unit.TryGetComponent<NavMeshAgent>(out var agent) && !agent.enabled)
            return false;

        return true;
    }

    #endregion

    #region Command Execution

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
            PlayMCCommandAnimation();
    }

    private bool HandleInteraction()
    {
        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();
        if (unit == null)
            return false;

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
        if (unit == null)
            return false;

        if (unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
                return false;
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
        if (unit == null)
            return false;

        if (unit.IsMainCharacter && MCFormController.Instance != null)
        {
            if (MCFormController.Instance.GetCurrentForm() == CharacterForm.Conductor)
                return false;
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
        if (unit == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mouseWorldLayerMask))
        {
            unit.HandleMovement(hit.point);
            ShowMoveIndicator(hit.point);
            return true;
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

        AudioManager.PlayOneShot(statsManager.Sounds.commandSound);

        if (unit != null && unit.enabled && !unit.IsMainCharacter)
        {
            yield return new WaitForSeconds(unitSoundDelay);

            if (unit != null && unit.enabled)
                unit.PlayCommandSound();
        }
    }

    private void PlayMCCommandAnimation()
    {
        if (Time.time - lastCommandAnimationTime < commandAnimationCooldown)
            return;

        Unit unit = UnitSelectionSystem.Instance.GetSelectedUnit();

        if (unit != null && !unit.IsMainCharacter)
        {
            Unit.MainCharacter?.PlayCommandAnimation();
            lastCommandAnimationTime = Time.time;
        }
    }

    private void ShowMoveIndicator(Vector3 position)
    {
        if (moveIndicatorPrefab != null)
            Instantiate(moveIndicatorPrefab, position + new Vector3(0f, indicatorYOffset, 0f), Quaternion.identity);
    }

    private void ShowActionFeedback(GameObject prefab, Transform target)
    {
        if (prefab != null && target != null)
            Instantiate(prefab, target.position, Quaternion.identity, target);
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

    #endregion
}