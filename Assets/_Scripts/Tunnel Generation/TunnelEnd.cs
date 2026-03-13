using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TunnelEnd : MonoBehaviour
{
    public static event Action OnTunnelEndReached;

    [SerializeField] private SwarmState swarmState;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private float energyRestorePercentage = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("TunnelEnd: Scene to load is not set!");
            return;
        }

        OnTunnelEndReached?.Invoke();
        RestoreEnergy();
        SceneManager.LoadScene(sceneToLoad);
    }

    private void RestoreEnergy()
    {
        if (swarmState == null) return;

        foreach (var unit in swarmState.SwarmUnits)
        {
            if (!unit.isAlive || unit.unitType == null) continue;

            float restoreAmount = unit.unitType.maxEnergy * energyRestorePercentage;
            unit.RestoreEnergy(restoreAmount);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;
    }
#endif
}