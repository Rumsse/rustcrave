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
    [SerializeField] private float spiderDropForwardOffset;
    [SerializeField] private float jumpArcHeight;
    [SerializeField] private Transform hatSlot;
    [SerializeField] private float returnSpeedMultiplier;
    [SerializeField] private float arrivalThreshold;

    [Header("Visuals")]
    [SerializeField] private GameObject conductorVisual;
    [SerializeField] private GameObject spiderVisual;

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
    private StatsManager statsManager;

    private Vector3 defaultSpiderLocalPos;
    private Quaternion defaultSpiderLocalRot;
    private Vector3 defaultConductorLocalPos;
    private Quaternion defaultConductorLocalRot;

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

    #endregion

    #region Form Switching Logic

    private void TryToggleForm()
    {
        if (TryGetComponent<Unit>(out var unit))
            unit.CancelActionAndPath();

        if (currentForm == CharacterForm.Conductor)
        {
            StartCoroutine(SwitchToSpiderRoutine());
            AudioManager.PlayOneShot(disconnectSound);
            CheckTutorialSpiderDetachAsync();
            return;
        }

        if (currentForm == CharacterForm.Spider)
        {
            StartCoroutine(SwitchToConductorRoutine());
            AudioManager.PlayOneShot(connectSound);
        }
    }

    private IEnumerator SwitchToSpiderRoutine()
    {
        isTransitioning = true;
        currentForm = CharacterForm.Spider;

        Vector3 startGlobalSpiderPos = hatSlot != null ? hatSlot.position : transform.position;

        droppedBodyInstance = Instantiate(conductorBodyPrefab, transform.position, transform.rotation);

        if (droppedBodyInstance.TryGetComponent<DroppedBody>(out var droppedBody))
        {
            if (droppedBody.hatObject != null)
                droppedBody.hatObject.SetActive(false);

            if (droppedBody.bodyAnimator != null)
                droppedBody.bodyAnimator.SetTrigger(bodyDropTriggerName);
        }
        else if (droppedBodyInstance.TryGetComponent<Animator>(out var fallbackAnimator))
            fallbackAnimator.SetTrigger(bodyDropTriggerName);

        SetFormVisuals(CharacterForm.Spider);
        ApplyStats(spiderStats);

        if (spiderAnimator != null)
            spiderAnimator.SetTrigger(disconnectTriggerName);

        Vector3 rootStart = transform.position;
        Vector3 rootTarget = transform.position + (transform.forward * spiderDropForwardOffset);
        float elapsed = 0f;

        if (TryGetComponent<NavMeshAgent>(out var agent))
            agent.enabled = false;

        while (elapsed < disconnectAnimationDuration)
        {
            float t = elapsed / disconnectAnimationDuration;

            transform.position = Vector3.Lerp(rootStart, rootTarget, t);

            Vector3 targetGlobalSpiderPos = transform.TransformPoint(defaultSpiderLocalPos);
            Vector3 linearPos = Vector3.Lerp(startGlobalSpiderPos, targetGlobalSpiderPos, t);

            linearPos.y += Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
            spiderVisual.transform.position = linearPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = rootTarget;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(rootTarget);
        }

        spiderVisual.transform.localPosition = defaultSpiderLocalPos;
        spiderVisual.transform.localRotation = defaultSpiderLocalRot;
        isTransitioning = false;
    }

    private IEnumerator SwitchToConductorRoutine()
    {
        if (droppedBodyInstance == null)
            yield break;

        isTransitioning = true;

        if (TryGetComponent<NavMeshAgent>(out var agent) && agent.enabled)
        {
            float originalSpeed = agent.speed;
            agent.speed = originalSpeed * returnSpeedMultiplier;

            agent.SetDestination(droppedBodyInstance.transform.position);

            if (spiderAnimator != null)
                spiderAnimator.SetBool(walkingBoolName, true);

            while (true)
            {
                if (!agent.pathPending)
                {
                    if (agent.remainingDistance <= arrivalThreshold)
                        break;
                    if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        Debug.LogWarning("NavMesh path invalid during Conductor return.");
                        break;
                    }
                }
                yield return null;
            }

            if (spiderAnimator != null)
                spiderAnimator.SetBool(walkingBoolName, false);

            agent.ResetPath();
            agent.speed = originalSpeed;
            agent.Warp(droppedBodyInstance.transform.position);
        }
        else
            transform.position = droppedBodyInstance.transform.position;

        transform.rotation = droppedBodyInstance.transform.rotation;

        if (spiderAnimator != null)
            spiderAnimator.SetTrigger(connectTriggerName);

        if (droppedBodyInstance.TryGetComponent<DroppedBody>(out var droppedBodyComponent))
        {
            if (droppedBodyComponent.bodyAnimator != null)
                droppedBodyComponent.bodyAnimator.SetTrigger(bodyRiseTriggerName);

            if (droppedBodyComponent.hatObject != null)
                droppedBodyComponent.hatObject.SetActive(true);
        }
        else if (droppedBodyInstance.TryGetComponent<Animator>(out var fallbackAnimator))
            fallbackAnimator.SetTrigger(bodyRiseTriggerName);

        yield return new WaitForSeconds(connectAnimationDuration);

        currentForm = CharacterForm.Conductor;

        Destroy(droppedBodyInstance);

        SetFormVisuals(CharacterForm.Conductor);
        ApplyStats(conductorStats);

        conductorVisual.transform.localPosition = defaultConductorLocalPos;
        conductorVisual.transform.localRotation = defaultConductorLocalRot;

        isTransitioning = false;
    }

    #endregion

    #region Utility Methods

    private void SetFormVisuals(CharacterForm form)
    {
        bool isConductor = form == CharacterForm.Conductor;

        if (conductorVisual != null)
            conductorVisual.SetActive(isConductor);
        if (spiderVisual != null)
            spiderVisual.SetActive(!isConductor);
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