using UnityEngine;
using System.Collections;
using System;

public class MCFormController : MonoBehaviour
{
    public event Action<UnitSO> OnFormChanged;

    [Header("Settings")]
    [SerializeField] private CharacterForm currentForm = CharacterForm.Conductor;
    [SerializeField] private KeyCode switchKey;
    [SerializeField] private GameObject conductorBodyPrefab;

    [Header("Visuals")]
    [SerializeField] private GameObject conductorVisual;
    [SerializeField] private GameObject spiderVisual;

    [Header("Stats")]
    [SerializeField] private UnitSO conductorStats;
    [SerializeField] private UnitSO spiderStats;

    [Header("Animations")]
    [SerializeField] private string disconnectTriggerName = "Disconnect";
    [SerializeField] private string connectTriggerName = "Connect";
    [SerializeField] private float connectAnimationDuration = 1f;

    private GameObject droppedBodyInstance;
    private Animator spiderAnimator;
    private bool isTransitioning = false;
    private StatsManager statsManager;

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
            SwitchToSpider();
        }
        else if (currentForm == CharacterForm.Spider)
        {
            StartCoroutine(SwitchToConductorRoutine());
        }
    }

    private void SwitchToSpider()
    {
        currentForm = CharacterForm.Spider;
        droppedBodyInstance = Instantiate(conductorBodyPrefab, transform.position, transform.rotation);

        SetFormVisuals(CharacterForm.Spider);
        ApplyStats(spiderStats);

        if (spiderAnimator != null)
        {
            spiderAnimator.SetTrigger(disconnectTriggerName);
        }
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

        if (TryGetComponent<MCMovement>(out var movementController))
        {
            movementController.SetSpeed(statsManager != null ? statsManager.MoveSpeed : newStats.moveSpeed);

            Animator activeAnimator = null;
            if (currentForm == CharacterForm.Conductor && conductorVisual != null)
            {
                activeAnimator = conductorVisual.GetComponent<Animator>();
            }
            else if (currentForm == CharacterForm.Spider && spiderVisual != null)
            {
                activeAnimator = spiderAnimator;
            }

            movementController.SetAnimator(activeAnimator);
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