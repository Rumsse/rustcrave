using System;
using UnityEngine;

public class TunnelEnd : MonoBehaviour
{

    #region Events

    public static event Action OnTunnelEndReached;

    #endregion

    #region Serialized Fields

    [SerializeField] private SwarmState swarmState;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private float energyRestorePercentage = 0.5f;

    #endregion

    #region Private Fields

    private bool isTransitioning;

    #endregion

    #region Unity Methods

    private async void OnTriggerEnter(Collider other)
    {
        if (isTransitioning)
            return;

        if (!other.CompareTag("Unit"))
            return;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("TunnelEnd: Scene to load is not set!");
            return;
        }

        isTransitioning = true;
        OnTunnelEndReached?.Invoke();
        RestoreEnergy();

        if (SceneTransitionManager.Instance != null)
            await SceneTransitionManager.Instance.FadeToScene(sceneToLoad);
        else
            Debug.LogError("TunnelEnd: SceneTransitionManager is missing!");
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

    private void RestoreEnergy()
    {
        if (swarmState == null)
            return;

        foreach (var unit in swarmState.SwarmUnits)
        {
            if (!unit.isAlive || unit.unitType == null)
                continue;

            unit.RestoreEnergy(unit.unitType.maxEnergy * energyRestorePercentage);
        }
    }

    #endregion
}