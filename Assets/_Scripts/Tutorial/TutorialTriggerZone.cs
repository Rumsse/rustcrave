using System.Collections.Generic;
using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    [SerializeField] private string unitTag = "Unit";
    [SerializeField] private bool completeTaskOnEnter = true;
    [SerializeField] private TutorialTaskType targetTaskType = TutorialTaskType.MoveUnits;
    [SerializeField] private GameObject visualEffect;

    [Header("Swarm Data")]
    [SerializeField] private SwarmState swarmState; 

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

        CheckCompletion();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(unitTag))
            return;

        if (!other.TryGetComponent<Unit>(out var unit))
            return;

        unitsInZone.RemoveWhere(u => u == null || !u.gameObject.activeInHierarchy);
        unitsInZone.Remove(unit);
    }

    private void CheckCompletion()
    {
        if (swarmState == null || unitsInZone.Count < swarmState.AliveCount)
            return;

        if (TutorialTaskVerifier.Instance != null && completeTaskOnEnter)
            TutorialTaskVerifier.Instance.CompleteTask();

        if (visualEffect != null)
            visualEffect.SetActive(false);

        gameObject.SetActive(false);
    }
}