using System;
using UnityEngine;

public class CheatUnitSpawner : MonoBehaviour
{
    public static event Action<UnitSO> OnCheatSpawnRequested;

    [SerializeField] private UnitSO minerUnit;
    [SerializeField] private UnitSO warriorUnit;
    [SerializeField] private UnitSO specialistUnit;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            TrySpawnUnit(minerUnit, "Miner");

        if (Input.GetKeyDown(KeyCode.O))
            TrySpawnUnit(warriorUnit, "Warrior");

        if (Input.GetKeyDown(KeyCode.P))
            TrySpawnUnit(specialistUnit, "Specialist");
    }

    void TrySpawnUnit(UnitSO unit, string unitName)
    {
        if (unit == null)
        {
            Debug.LogError($"Cheat failed: UnitSO for {unitName} is not assigned!");
            return;
        }

        OnCheatSpawnRequested?.Invoke(unit);
        Debug.Log($"Cheat activated: Spawned {unitName}");
    }
}