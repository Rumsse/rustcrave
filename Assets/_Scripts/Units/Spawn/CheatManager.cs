using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [SerializeField] private SwarmState swarmState;

    void OnEnable() => CheatUnitSpawner.OnCheatSpawnRequested += HandleCheatSpawn;

    void OnDisable() => CheatUnitSpawner.OnCheatSpawnRequested -= HandleCheatSpawn;

    void HandleCheatSpawn(UnitData unit)
    {
        if (swarmState == null)
        {
            Debug.LogError("SwarmState is missing in CheatManager!");
            return;
        }

        swarmState.AddUnitToSwarm(unit);
        Debug.Log($"Cheat applied: Added {unit.name} directly to swarm.");
    }
}