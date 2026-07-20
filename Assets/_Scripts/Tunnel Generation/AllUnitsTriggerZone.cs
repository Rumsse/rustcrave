using System;
using System.Collections.Generic;
using UnityEngine;

public class AllUnitsTriggerZone : MonoBehaviour
{
    public static event Action OnTunnelEndReached;

    [SerializeField] private SwarmState swarmState;
    [SerializeField] private EventState eventState;
    [SerializeField, SceneName] private string sceneToLoad;
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private float energyRestorePercentage = 0.5f;

    private HashSet<Unit> unitsInZone = new();
    private bool isTransitioning;


    #region Unity Methods

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning)
            return;

        if (!other.CompareTag("Unit"))
            return;

        if (!other.TryGetComponent<Unit>(out var unit))
            return;

        unitsInZone.Add(unit);
        CheckCompletion();
    }

    private void OnTriggerExit(Collider other)
    {
        if (isTransitioning)
            return;

        if (!other.CompareTag("Unit"))
            return;

        if (!other.TryGetComponent<Unit>(out var unit))
            return;

        unitsInZone.RemoveWhere(u => u == null || !u.gameObject.activeInHierarchy);
        unitsInZone.Remove(unit);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;
    }
#endif

    #endregion

    #region Logic

    private async void CheckCompletion()
    {
        if (swarmState == null)
            return;

        if (unitsInZone.Count < swarmState.AliveCount)
            return;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[AllUnitsTriggerZone] Scene to load is not set!");
            return;
        }

        isTransitioning = true;

        foreach (var unit in unitsInZone)
        {
            if (unit != null)
                unit.SyncDataToState();
        }

        if (eventState != null)
            eventState.Reset();

        OnTunnelEndReached?.Invoke();
        RestoreEnergy();

        if (TutorialManager.Instance == null)
            SaveManager.Instance.AutoSaveGame();

        if (SceneTransitionManager.Instance != null)
            await SceneTransitionManager.Instance.FadeToScene(sceneToLoad);
        else
            Debug.LogError("[AllUnitsTriggerZone] SceneTransitionManager is missing!");
    }

    private void RestoreEnergy()
    {
        if (swarmState == null || swarmState.SwarmUnits == null)
            return;

        foreach (var data in swarmState.SwarmUnits)
        {
            if (data == null || !data.isAlive || data.unitType == null)
                continue;

            data.RestoreEnergy(data.unitType.maxEnergy * energyRestorePercentage);
        }
    }

    #endregion
}