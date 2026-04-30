using UnityEngine;
using System.Collections.Generic;

public class TutorialTriggerZone : MonoBehaviour
{
    [SerializeField] private string unitTag = "Unit";
    [SerializeField] private bool completeTaskOnEnter = true;
    [SerializeField] private TutorialTaskType targetTaskType = TutorialTaskType.MoveUnits;
    [SerializeField] private GameObject visualEffect;
    [SerializeField] private int requiredUnitsCount = 4;

    private HashSet<Unit> unitsInZone = new();

    private void Start()
    {
        if (visualEffect != null)
            visualEffect.SetActive(false);

        if (TutorialTaskVerifier.Instance == null)
            return;

        TutorialTaskVerifier.Instance.OnTaskStarted += HandleTaskStarted;
        TutorialTaskVerifier.Instance.OnTaskEnded += HandleTaskEnded;
    }

    private void OnDestroy()
    {
        if (TutorialTaskVerifier.Instance == null)
            return;

        TutorialTaskVerifier.Instance.OnTaskStarted -= HandleTaskStarted;
        TutorialTaskVerifier.Instance.OnTaskEnded -= HandleTaskEnded;
    }

    private void HandleTaskStarted(TutorialTaskType task)
    {
        if (task == targetTaskType && visualEffect != null)
            visualEffect.SetActive(true);
    }

    private void HandleTaskEnded(TutorialTaskType task)
    {
        if (task == targetTaskType && visualEffect != null)
            visualEffect.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(unitTag))
            return;

        if (!other.TryGetComponent<Unit>(out var unit))
            return;

        unitsInZone.Add(unit);
        //Debug.Log($"[TutorialTriggerZone] Unit entered. Count: {unitsInZone.Count}/{requiredUnitsCount}");

        CheckCompletion();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(unitTag))
            return;

        if (!other.TryGetComponent<Unit>(out var unit))
            return;

        unitsInZone.Remove(unit);
        //Debug.Log($"[TutorialTriggerZone] Unit left. Count: {unitsInZone.Count}/{requiredUnitsCount}");
    }

    private void CheckCompletion()
    {
        if (unitsInZone.Count < requiredUnitsCount)
            return;

        //Debug.Log("[TutorialTriggerZone] All required units are in the zone. Completing task.");

        if (TutorialTaskVerifier.Instance != null && completeTaskOnEnter)
            TutorialTaskVerifier.Instance.CompleteTask();

        if (visualEffect != null)
            visualEffect.SetActive(false);

        gameObject.SetActive(false);
    }
}