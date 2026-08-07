using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using FMODUnity;

public enum CharacterForm
{
    Conductor,
    Spider
}

public class MCFormController : MonoBehaviour
{
    #region Properties & Fields

    public static MCFormController Instance { get; private set; }

    public event Action<UnitData> OnFormChanged;
    public bool IsTransitioning => isTransitioning;

    [Header("Settings")]
    [SerializeField] private CharacterForm currentForm = CharacterForm.Conductor;
    [SerializeField] private GameObject conductorBodyPrefab;
    [SerializeField] private string returnTargetBoneName = "CAP_on_head_pose (1)";
    [SerializeField] private float spiderDropForwardOffset;
    [SerializeField] private float jumpArcHeight;
    [SerializeField] private Transform hatSlot;
    [SerializeField] private float returnSpeedMultiplier;
    [SerializeField] private float arrivalThreshold;
    [SerializeField] private float maxReturnTime = 5f;

    [Header("Visuals")]
    [SerializeField] private GameObject conductorVisual;
    [SerializeField] private GameObject spiderVisual;
    [SerializeField] private GameObject activeHat;

    [Header("Stats")]
    [SerializeField] private UnitData conductorStats;
    [SerializeField] private UnitData spiderStats;

    [Header("Sounds")]
    [SerializeField] private EventReference connectSound;
    [SerializeField] private EventReference disconnectSound;

    [Header("Animations")]
    [SerializeField] private string disconnectTriggerName = "Disconnect";
    [SerializeField] private string connectTriggerName = "Connect";
    [SerializeField] private string bodyDropTriggerName = "Down";
    [SerializeField] private string bodyRiseTriggerName = "Up";
    [SerializeField] private string walkingBoolName = "IsWalking";
    [SerializeField] private float connectAnimationDuration;
    [SerializeField] private float disconnectAnimationDuration;

    private const float TUTORIAL_DETACH_DELAY = 1f;
    private const int DEFAULT_AVOIDANCE_PRIORITY = 50;

    private PlayerControls input;
    private GameObject droppedBodyInstance;
    private Animator spiderAnimator;
    private bool isTransitioning;
    private bool isWalkingToBody;
    private Coroutine transitionCoroutine;
    private StatsManager statsManager;

    private Vector3 defaultSpiderLocalPos;
    private Quaternion defaultSpiderLocalRot;
    private Vector3 defaultConductorLocalPos;
    private Quaternion defaultConductorLocalRot;

    private bool isOverridingVisualPos;
    private Vector3 currentSpiderVisualPos;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;
        input = new PlayerControls();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Gameplay.SwitchForm.performed += OnSwitchFormPerformed;
        input.Gameplay.Move.performed += OnMovePerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Gameplay.SwitchForm.performed -= OnSwitchFormPerformed;
        input.Gameplay.Move.performed -= OnMovePerformed;
    }

    private void Start()
    {
        statsManager = GetComponent<StatsManager>();

        if (spiderVisual)
        {
            spiderAnimator = spiderVisual.GetComponent<Animator>();
            defaultSpiderLocalPos = spiderVisual.transform.localPosition;
            defaultSpiderLocalRot = spiderVisual.transform.localRotation;
        }

        if (conductorVisual)
        {
            defaultConductorLocalPos = conductorVisual.transform.localPosition;
            defaultConductorLocalRot = conductorVisual.transform.localRotation;
        }

        SetFormVisuals(currentForm);
        ApplyStats(currentForm == CharacterForm.Conductor ? conductorStats : spiderStats);
    }

    private void LateUpdate()
    {
        if (isOverridingVisualPos && spiderVisual)
            spiderVisual.transform.position = currentSpiderVisualPos;
    }

    #endregion

    #region Input Handlers

    private void OnSwitchFormPerformed(InputAction.CallbackContext context)
    {
        if (isTransitioning || Time.timeScale == 0f)
            return;

        TryToggleForm();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isWalkingToBody)
            CancelReturn();
    }

    #endregion

    #region Form Switching Logic

    private void TryToggleForm()
    {
        if (TryGetComponent<Unit>(out var unit))
            unit.CancelActionAndPath();

        if (currentForm == CharacterForm.Conductor)
        {
            transitionCoroutine = StartCoroutine(SwitchToSpiderRoutine());
            AudioManager.PlayOneShot(disconnectSound);
            CheckTutorialSpiderDetachAsync();
            return;
        }

        if (currentForm == CharacterForm.Spider)
        {
            transitionCoroutine = StartCoroutine(SwitchToConductorRoutine());
            AudioManager.PlayOneShot(connectSound);
        }
    }

    public void CancelReturn()
    {
        if (!isWalkingToBody || transitionCoroutine == null)
            return;

        StopCoroutine(transitionCoroutine);
        transitionCoroutine = null;

        isWalkingToBody = false;
        isTransitioning = false;
        isOverridingVisualPos = false;

        if (TryGetComponent<MCMovement>(out var mcMovement))
            mcMovement.enabled = true;

        if (spiderAnimator)
            spiderAnimator.SetBool(walkingBoolName, false);

        if (spiderVisual)
        {
            spiderVisual.transform.localPosition = defaultSpiderLocalPos;
            spiderVisual.transform.localRotation = defaultSpiderLocalRot;
        }

        if (!TryGetComponent<NavMeshAgent>(out var agent))
            return;

        agent.ResetPath();
        agent.speed = statsManager ? statsManager.MoveSpeed : spiderStats.moveSpeed;
        agent.avoidancePriority = DEFAULT_AVOIDANCE_PRIORITY;
    }

    private IEnumerator SwitchToSpiderRoutine()
    {
        if (disconnectAnimationDuration <= 0f)
            Debug.LogWarning("[MCFormController] DisconnectAnimationDuration is 0 or less. Arc will be skipped!");

        isTransitioning = true;
        currentForm = CharacterForm.Spider;

        if (TryGetComponent<MCMovement>(out var mcMovement))
            mcMovement.enabled = false;

        Vector3 visualStartGlobalPos = hatSlot ? hatSlot.position : transform.position + Vector3.up * 1.8f;
        droppedBodyInstance = Instantiate(conductorBodyPrefab, transform.position, transform.rotation);

        if (droppedBodyInstance.TryGetComponent<BoxCollider>(out var boxCollider))
            boxCollider.enabled = true;

        ToggleHatOnClone(droppedBodyInstance, false);

        if (droppedBodyInstance.TryGetComponent<DroppedBody>(out var droppedBody))
            droppedBody.bodyAnimator?.SetTrigger(bodyDropTriggerName);
        else if (droppedBodyInstance.TryGetComponent<Animator>(out var fallbackAnimator))
            fallbackAnimator.SetTrigger(bodyDropTriggerName);

        SetFormVisuals(CharacterForm.Spider);
        ApplyStats(spiderStats);

        if (spiderAnimator)
            spiderAnimator.SetTrigger(disconnectTriggerName);

        Vector3 rootStartPos = transform.position;
        Vector3 rootTargetPos = transform.position + (transform.forward * spiderDropForwardOffset);
        float elapsed = 0f;

        if (TryGetComponent<NavMeshAgent>(out var agent))
            agent.enabled = false;

        isOverridingVisualPos = true;

        while (elapsed < disconnectAnimationDuration)
        {
            float t = elapsed / disconnectAnimationDuration;

            transform.position = Vector3.Lerp(rootStartPos, rootTargetPos, t);

            Vector3 visualTargetGlobalPos = transform.TransformPoint(defaultSpiderLocalPos);
            Vector3 currentVisualPos = Vector3.Lerp(visualStartGlobalPos, visualTargetGlobalPos, t);

            float lerpedHeight = Mathf.Lerp(visualStartGlobalPos.y, visualTargetGlobalPos.y, t);
            currentVisualPos.y = lerpedHeight + (Mathf.Sin(t * Mathf.PI) * jumpArcHeight);

            currentSpiderVisualPos = currentVisualPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        isOverridingVisualPos = false;
        transform.position = rootTargetPos;

        if (agent)
        {
            agent.enabled = true;
            agent.Warp(rootTargetPos);
        }

        spiderVisual.transform.localPosition = defaultSpiderLocalPos;
        spiderVisual.transform.localRotation = defaultSpiderLocalRot;
        isTransitioning = false;

        if (mcMovement)
            mcMovement.enabled = true;
    }

    private IEnumerator SwitchToConductorRoutine()
    {
        if (!droppedBodyInstance)
            yield break;

        if (connectAnimationDuration <= 0f)
            Debug.LogWarning("[MCFormController] ConnectAnimationDuration is 0 or less. Arc will be skipped!");

        isTransitioning = true;
        isWalkingToBody = true;

        if (TryGetComponent<MCMovement>(out var mcMovement))
            mcMovement.enabled = false;

        if (droppedBodyInstance.TryGetComponent<Collider>(out var bodyCol))
            bodyCol.enabled = false;

        Transform cloneTargetSlot = GetCloneTargetSlot(droppedBodyInstance);

        if (TryGetComponent<NavMeshAgent>(out var agent) && agent.enabled)
        {
            float originalSpeed = agent.speed;
            int originalPriority = agent.avoidancePriority;

            agent.speed = originalSpeed * returnSpeedMultiplier;
            agent.avoidancePriority = 0;

            Vector3 targetPos = cloneTargetSlot.position;
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                targetPos = hit.position;

            agent.SetDestination(targetPos);

            if (spiderAnimator)
                spiderAnimator.SetBool(walkingBoolName, true);

            float currentReturnTime = 0f;

            while (isWalkingToBody)
            {
                if (!agent.pathPending)
                {
                    float dist2D = Vector2.Distance(
                        new Vector2(transform.position.x, transform.position.z),
                        new Vector2(targetPos.x, targetPos.z)
                    );

                    if (dist2D <= arrivalThreshold || (agent.hasPath && agent.remainingDistance <= arrivalThreshold))
                        break;

                    if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        Debug.LogWarning("[MCFormController] Path invalid. Forcing connection.");
                        break;
                    }
                }

                currentReturnTime += Time.deltaTime;

                if (currentReturnTime >= maxReturnTime)
                {
                    Debug.LogWarning("[MCFormController] Return to body timed out. Forcing connection.");
                    break;
                }

                if (currentReturnTime > 0.5f && agent.velocity.sqrMagnitude < 0.01f)
                {
                    float dist2D = Vector2.Distance(
                        new Vector2(transform.position.x, transform.position.z),
                        new Vector2(targetPos.x, targetPos.z)
                    );

                    if (dist2D > arrivalThreshold * 2f)
                    {
                        Debug.LogWarning("[MCFormController] Agent stuck while returning. Forcing connection.");
                        break;
                    }
                }

                yield return null;
            }

            if (!isWalkingToBody)
            {
                if (mcMovement) mcMovement.enabled = true;
                yield break;
            }

            isWalkingToBody = false;

            if (spiderAnimator)
                spiderAnimator.SetBool(walkingBoolName, false);

            agent.ResetPath();
            agent.speed = originalSpeed;
            agent.avoidancePriority = originalPriority;
            agent.enabled = false;
        }
        else
        {
            isWalkingToBody = false;
            transform.position = cloneTargetSlot.position;
        }

        if (spiderAnimator)
            spiderAnimator.SetTrigger(connectTriggerName);

        ToggleHatOnClone(droppedBodyInstance, true);

        if (droppedBodyInstance.TryGetComponent<DroppedBody>(out var droppedBodyComponent))
            droppedBodyComponent.bodyAnimator?.SetTrigger(bodyRiseTriggerName);
        else if (droppedBodyInstance.TryGetComponent<Animator>(out var fallbackAnimator))
            fallbackAnimator.SetTrigger(bodyRiseTriggerName);

        Vector3 visualStartGlobalPos = transform.TransformPoint(defaultSpiderLocalPos);
        Vector3 rootStartPos = transform.position;
        Quaternion rootStartRot = transform.rotation;
        float elapsed = 0f;

        isOverridingVisualPos = true;

        while (elapsed < connectAnimationDuration)
        {
            float t = elapsed / connectAnimationDuration;

            transform.position = Vector3.Lerp(rootStartPos, droppedBodyInstance.transform.position, t);
            transform.rotation = Quaternion.Slerp(rootStartRot, droppedBodyInstance.transform.rotation, t);

            Vector3 visualTargetGlobalPos = cloneTargetSlot.position;
            Vector3 currentVisualPos = Vector3.Lerp(visualStartGlobalPos, visualTargetGlobalPos, t);

            float lerpedHeight = Mathf.Lerp(visualStartGlobalPos.y, visualTargetGlobalPos.y, t);
            currentVisualPos.y = lerpedHeight + (Mathf.Sin(t * Mathf.PI) * jumpArcHeight);

            currentSpiderVisualPos = currentVisualPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        isOverridingVisualPos = false;
        currentForm = CharacterForm.Conductor;

        transform.position = droppedBodyInstance.transform.position;
        transform.rotation = droppedBodyInstance.transform.rotation;

        if (agent)
        {
            agent.Warp(droppedBodyInstance.transform.position);
            agent.enabled = true;
        }

        Destroy(droppedBodyInstance);

        SetFormVisuals(CharacterForm.Conductor);
        ApplyStats(conductorStats);

        conductorVisual.transform.localPosition = defaultConductorLocalPos;
        conductorVisual.transform.localRotation = defaultConductorLocalRot;

        spiderVisual.transform.localPosition = defaultSpiderLocalPos;
        spiderVisual.transform.localRotation = defaultSpiderLocalRot;

        isTransitioning = false;

        if (mcMovement)
            mcMovement.enabled = true;
    }

    #endregion

    #region Utility Methods

    private Transform GetCloneTargetSlot(GameObject clone)
    {
        if (!string.IsNullOrEmpty(returnTargetBoneName))
        {
            foreach (Transform child in clone.GetComponentsInChildren<Transform>(true))
                if (child.name == returnTargetBoneName)
                    return child;
        }

        if (hatSlot)
        {
            foreach (Transform child in clone.GetComponentsInChildren<Transform>(true))
                if (child.name == hatSlot.name)
                    return child;
        }

        return clone.transform;
    }

    private void SetFormVisuals(CharacterForm form)
    {
        bool isConductor = form == CharacterForm.Conductor;

        if (conductorVisual)
            conductorVisual.SetActive(isConductor);

        if (spiderVisual)
            spiderVisual.SetActive(!isConductor);

        if (activeHat)
            activeHat.SetActive(isConductor);
    }

    private void ToggleHatOnClone(GameObject clone, bool state)
    {
        if (!activeHat)
            return;

        foreach (Transform child in clone.GetComponentsInChildren<Transform>(true))
        {
            if (child.name != activeHat.name)
                continue;

            child.gameObject.SetActive(state);
            break;
        }
    }

    private void ApplyStats(UnitData newStats)
    {
        if (statsManager)
            statsManager.ChangeStats(newStats);

        Animator activeAnimator = currentForm == CharacterForm.Conductor && conductorVisual ? conductorVisual.GetComponent<Animator>() : spiderAnimator;

        if (TryGetComponent<MCMovement>(out var movementController))
        {
            movementController.SetSpeed(statsManager ? statsManager.MoveSpeed : newStats.moveSpeed);
            movementController.SetAnimator(activeAnimator);
        }

        if (TryGetComponent<Unit>(out var unit))
            unit.SetAnimator(activeAnimator);

        OnFormChanged?.Invoke(newStats);
    }

    public CharacterForm GetCurrentForm() => currentForm;

    private async void CheckTutorialSpiderDetachAsync()
    {
        if (TutorialTaskVerifier.Instance == null || TutorialTaskVerifier.Instance.CurrentTask != TutorialTaskType.DetachSpider)
            return;

        await Awaitable.WaitForSecondsAsync(TUTORIAL_DETACH_DELAY);

        if (TutorialTaskVerifier.Instance != null && TutorialTaskVerifier.Instance.CurrentTask == TutorialTaskType.DetachSpider)
            TutorialTaskVerifier.Instance.CompleteTask();
    }

    #endregion
}