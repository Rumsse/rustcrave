using UnityEngine;
using System.Collections;
using System;

public class MCFormController : MonoBehaviour
{
    public static MCFormController Instance { get; private set; }

    public event Action<UnitSO> OnFormChanged;

    [Header("Settings")]
    [SerializeField] private CharacterForm currentForm = CharacterForm.Conductor;
    [SerializeField] private KeyCode switchKey;
    [SerializeField] private GameObject conductorBodyPrefab;
    [SerializeField] private float spiderDropForwardOffset;
    [SerializeField] private Transform hatSlot;

    [Header("Visuals")]
    [SerializeField] private GameObject conductorVisual;
    [SerializeField] private GameObject spiderVisual;

    [Header("Stats")]
    [SerializeField] private UnitSO conductorStats;
    [SerializeField] private UnitSO spiderStats;

    [Header("Animations")]
    [SerializeField] private string disconnectTriggerName = "Disconnect";
    [SerializeField] private string connectTriggerName = "Connect";
    [SerializeField] private string bodyDropTriggerName = "Down";
    [SerializeField] private string bodyRiseTriggerName = "Up";
    [SerializeField] private float connectAnimationDuration;
    [SerializeField] private float disconnectAnimationDuration = 0.5f;

    private GameObject droppedBodyInstance;
    private Animator spiderAnimator;
    private bool isTransitioning = false;
    private StatsManager statsManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        statsManager = GetComponent<StatsManager>();

        if (spiderVisual != null)
        {
            spiderAnimator = spiderVisual.GetComponent<Animator>();
        }

        SetFormVisuals(currentForm);
        ApplyStats(currentForm == CharacterForm.Conductor ? conductorStats : spiderStats);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey) && !isTransitioning)
        {
            TryToggleForm();
        }
    }

    private void TryToggleForm()
    {
        if (currentForm == CharacterForm.Conductor)
        {
            StartCoroutine(SwitchToSpiderRoutine());
        }
        else if (currentForm == CharacterForm.Spider)
        {
            StartCoroutine(SwitchToConductorRoutine());
        }
    }

    private IEnumerator SwitchToSpiderRoutine()
    {
        isTransitioning = true;
        currentForm = CharacterForm.Spider;

        Vector3 startGlobalSpiderPos = hatSlot != null ? hatSlot.position : transform.position + Vector3.up * 2f;

        droppedBodyInstance = Instantiate(conductorBodyPrefab, transform.position, transform.rotation);

        if (droppedBodyInstance.TryGetComponent<DroppedBody>(out var droppedBody))
        {
            if (droppedBody.hatObject != null)
            {
                droppedBody.hatObject.SetActive(false);
            }

            if (droppedBody.bodyAnimator != null)
            {
                droppedBody.bodyAnimator.SetTrigger(bodyDropTriggerName);
            }
        }
        else if (droppedBodyInstance.TryGetComponent<Animator>(out var fallbackAnimator))
        {
            fallbackAnimator.SetTrigger(bodyDropTriggerName);
        }

        SetFormVisuals(CharacterForm.Spider);
        ApplyStats(spiderStats);

        if (spiderAnimator != null)
        {
            spiderAnimator.SetTrigger(disconnectTriggerName);
        }

        Vector3 rootStart = transform.position;
        Vector3 rootTarget = transform.position + (transform.forward * spiderDropForwardOffset);

        Vector3 defaultSpiderLocal = spiderVisual.transform.localPosition;

        float elapsed = 0f;

        while (elapsed < disconnectAnimationDuration)
        {
            float t = elapsed / disconnectAnimationDuration;

            transform.position = Vector3.Lerp(rootStart, rootTarget, t);

            Vector3 targetGlobalSpiderPos = transform.TransformPoint(defaultSpiderLocal);
            spiderVisual.transform.position = Vector3.Lerp(startGlobalSpiderPos, targetGlobalSpiderPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = rootTarget;
        spiderVisual.transform.localPosition = defaultSpiderLocal;
        isTransitioning = false;
    }

    private IEnumerator SwitchToConductorRoutine()
    {
        if (droppedBodyInstance == null) yield break;

        isTransitioning = true;

        transform.position = droppedBodyInstance.transform.position;
        transform.rotation = droppedBodyInstance.transform.rotation;

        if (spiderAnimator != null)
        {
            spiderAnimator.SetTrigger(connectTriggerName);
        }

        if (droppedBodyInstance.TryGetComponent<DroppedBody>(out var droppedBody))
        {
            if (droppedBody.bodyAnimator != null)
            {
                droppedBody.bodyAnimator.SetTrigger(bodyRiseTriggerName);
            }

            if (droppedBody.hatObject != null)
            {
                droppedBody.hatObject.SetActive(true);
            }
        }
        else if (droppedBodyInstance.TryGetComponent<Animator>(out var fallbackAnimator))
        {
            fallbackAnimator.SetTrigger(bodyRiseTriggerName);
        }

        yield return new WaitForSeconds(connectAnimationDuration);

        currentForm = CharacterForm.Conductor;

        Destroy(droppedBodyInstance);

        SetFormVisuals(CharacterForm.Conductor);
        ApplyStats(conductorStats);

        isTransitioning = false;
    }

    private void SetFormVisuals(CharacterForm form)
    {
        bool isConductor = (form == CharacterForm.Conductor);
        if (conductorVisual != null) conductorVisual.SetActive(isConductor);
        if (spiderVisual != null) spiderVisual.SetActive(!isConductor);
    }

    private void ApplyStats(UnitSO newStats)
    {
        if (statsManager != null)
        {
            statsManager.ChangeStats(newStats);
        }

        Animator activeAnimator = null;
        if (currentForm == CharacterForm.Conductor && conductorVisual != null)
        {
            activeAnimator = conductorVisual.GetComponent<Animator>();
        }
        else if (currentForm == CharacterForm.Spider && spiderVisual != null)
        {
            activeAnimator = spiderAnimator;
        }

        if (TryGetComponent<MCMovement>(out var movementController))
        {
            movementController.SetSpeed(statsManager != null ? statsManager.MoveSpeed : newStats.moveSpeed);
            movementController.SetAnimator(activeAnimator);
        }

        if (TryGetComponent<Unit>(out var unit))
        {
            unit.SetAnimator(activeAnimator);
        }

        OnFormChanged?.Invoke(newStats);
    }

    public CharacterForm GetCurrentForm()
    {
        return currentForm;
    }
}

public enum CharacterForm
{
    Conductor,
    Spider
}