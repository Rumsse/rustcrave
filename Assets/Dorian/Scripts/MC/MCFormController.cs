using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;
using FMODUnity;

public class MCFormController : MonoBehaviour
{
    #region Properties & Fields

    public static MCFormController Instance { get; private set; }

    public event Action<UnitSO> OnFormChanged;
    public bool IsTransitioning => isTransitioning;

    [Header("Settings")]
    [SerializeField] private CharacterForm currentForm = CharacterForm.Conductor;
    [SerializeField] private KeyCode switchKey;
    [SerializeField] private GameObject conductorBodyPrefab;
    [SerializeField] private string returnTargetBoneName = "CAP_on_head_pose (1)";
    [SerializeField] private float spiderDropForwardOffset;
    [SerializeField] private float jumpArcHeight;
    [SerializeField] private Transform hatSlot;
    [SerializeField] private float returnSpeedMultiplier;
    [SerializeField] private float arrivalThreshold;

    [Header("Visuals")]
    [SerializeField] private GameObject conductorVisual;
    [SerializeField] private GameObject spiderVisual;
    [SerializeField] private GameObject activeHat;

    [Header("Stats")]
    [SerializeField] private UnitSO conductorStats;
    [SerializeField] private UnitSO spiderStats;

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

    private GameObject droppedBodyInstance;
    private Animator spiderAnimator;
    private bool isTransitioning = false;
    private bool isWalkingToBody = false;
    private Coroutine transitionCoroutine;
    private StatsManager statsManager;

    private Vector3 defaultSpiderLocalPos;
    private Quaternion defaultSpiderLocalRot;
    private Vector3 defaultConductorLocalPos;
    private Quaternion defaultConductorLocalRot;

    private bool isOverridingVisualPos = false;
    private Vector3 currentSpiderVisualPos;

    #endregion

    #region Unity Methods

    private void Awake() => Instance = this;

    private void Start()
    {
        statsManager = GetComponent<StatsManager>();

        if (spiderVisual != null)
        {
            spiderAnimator = spiderVisual.GetComponent<Animator>();
            defaultSpiderLocalPos = spiderVisual.transform.localPosition;
            defaultSpiderLocalRot = spiderVisual.transform.localRotation;
        }

        if (conductorVisual != null)
        {
            defaultConductorLocalPos = conductorVisual.transform.localPosition;
            defaultConductorLocalRot = conductorVisual.transform.localRotation;
        }

        SetFormVisuals(currentForm);
        ApplyStats(currentForm == CharacterForm.Conductor ? conductorStats : spiderStats);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey) && !isTransitioning)
            TryToggleForm();
    }

    private void LateUpdate()
    {
        if (isOverridingVisualPos && spiderVisual != null)
            spiderVisual.transform.position = currentSpiderVisualPos;
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

        if (spiderAnimator != null)
            spiderAnimator.SetBool(walkingBoolName, false);

        if (spiderVisual != null)
        {
            spiderVisual.transform.localPosition = defaultSpiderLocalPos;
            spiderVisual.transform.localRotation = defaultSpiderLocalRot;
        }

        if (!TryGetComponent<NavMeshAgent>(out var agent))
            return;

        agent.ResetPath();
        agent.speed = statsManager != null ? statsManager.MoveSpeed : spiderStats.moveSpeed;
    }

    private IEnumerator SwitchToSpiderRoutine()
    {
        if (disconnectAnimationDuration <= 0f)
            Debug.LogWarning("DisconnectAnimationDuration is 0 or less. Arc will be skipped!");

        isTransitioning = true;
        currentForm = CharacterForm.Spider;

        Vector3 visualStartGlobalPos = hatSlot != null ? hatSlot.position : transform.position + Vector3.up * 1.8f;

        droppedBodyInstance = Instantiate(conductorBodyPrefab, transform.position, transform.rotation);

        if (droppedBodyInstance.TryGetComponent<BoxCollider>(out var boxCollider))
            boxCollider.enabled = true;

        DisableHatOnClone(droppedBodyInstance);

        if (droppedBodyInstance.TryGetComponent<DroppedBody>(out var droppedBody))
            droppedBody.bodyAnimator?.SetTrigger(bodyDropTriggerName);
        else if (droppedBodyInstance.TryGetComponent<Animator>(out var fallbackAnimator))
            fallbackAnimator.SetTrigger(bodyDropTriggerName);

        SetFormVisuals(CharacterForm.Spider);
        ApplyStats(spiderStats);

        if (spiderAnimator != null)
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

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(rootTargetPos);
        }

        spiderVisual.transform.localPosition = defaultSpiderLocalPos;
        spiderVisual.transform.localRotation = defaultSpiderLocalRot;
        isTransitioning = false;
    }

    private IEnumerator SwitchToConductorRoutine()
    {
        if (droppedBodyInstance == null)
            yield break;

        if (connectAnimationDuration <= 0f)
            Debug.LogWarning("ConnectAnimationDuration is 0 or less. Arc will be skipped!");

        isTransitioning = true;
        isWalkingToBody = true;

        Transform cloneTargetSlot = GetCloneTargetSlot(droppedBodyInstance);

        if (TryGetComponent<NavMeshAgent>(out var agent) && agent.enabled)
        {
            float originalSpeed = agent.speed;
            agent.speed = originalSpeed * returnSpeedMultiplier;

            agent.SetDestination(cloneTargetSlot.position);

            if (spiderAnimator != null)
                spiderAnimator.SetBool(walkingBoolName, true);

            while (true)
            {
                if (!agent.pathPending)
                {
                    if (agent.remainingDistance <= arrivalThreshold)
                        break;
                    if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                        break;
                }
                yield return null;
            }

            isWalkingToBody = false;

            if (spiderAnimator != null)
                spiderAnimator.SetBool(walkingBoolName, false);

            agent.ResetPath();
            agent.speed = originalSpeed;
            agent.enabled = false; 
        }
        else
        {
            isWalkingToBody = false;
            transform.position = cloneTargetSlot.position;
        }

        if (spiderAnimator != null)
            spiderAnimator.SetTrigger(connectTriggerName);

        EnableHatOnClone(droppedBodyInstance);

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

        if (agent != null)
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
    }

    #endregion

    #region Utility Methods

    private Transform GetCloneTargetSlot(GameObject clone)
    {
        if (!string.IsNullOrEmpty(returnTargetBoneName))
        {
            Transform[] allTransforms = clone.GetComponentsInChildren<Transform>(true);
            foreach (Transform childTransform in allTransforms)
            {
                if (childTransform.name == returnTargetBoneName)
                    return childTransform;
            }
        }

        if (hatSlot != null)
        {
            Transform[] allTransforms = clone.GetComponentsInChildren<Transform>(true);
            foreach (Transform childTransform in allTransforms)
            {
                if (childTransform.name == hatSlot.name)
                    return childTransform;
            }
        }

        return clone.transform;
    }

    private void SetFormVisuals(CharacterForm form)
    {
        bool isConductor = form == CharacterForm.Conductor;

        if (conductorVisual != null)
            conductorVisual.SetActive(isConductor);

        if (spiderVisual != null)
            spiderVisual.SetActive(!isConductor);

        if (activeHat != null)
            activeHat.SetActive(isConductor);
    }

    private void DisableHatOnClone(GameObject clone)
    {
        if (activeHat == null)
            return;

        Transform[] allTransforms = clone.GetComponentsInChildren<Transform>(true);
        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.name != activeHat.name)
                continue;

            childTransform.gameObject.SetActive(false);
            break;
        }
    }

    private void EnableHatOnClone(GameObject clone)
    {
        if (activeHat == null)
            return;

        Transform[] allTransforms = clone.GetComponentsInChildren<Transform>(true);
        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.name != activeHat.name)
                continue;

            childTransform.gameObject.SetActive(true);
            break;
        }
    }

    private void ApplyStats(UnitSO newStats)
    {
        if (statsManager != null)
            statsManager.ChangeStats(newStats);

        Animator activeAnimator = null;

        if (currentForm == CharacterForm.Conductor && conductorVisual != null)
            activeAnimator = conductorVisual.GetComponent<Animator>();
        else if (currentForm == CharacterForm.Spider && spiderVisual != null)
            activeAnimator = spiderAnimator;

        if (TryGetComponent<MCMovement>(out var movementController))
        {
            movementController.SetSpeed(statsManager != null ? statsManager.MoveSpeed : newStats.moveSpeed);
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

public enum CharacterForm
{
    Conductor,
    Spider
}