using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobotHUDManager : MonoBehaviour
{
    [SerializeField] private SwarmState swarmState;
    [SerializeField] private GameObject robotHUDPrefab;
    [SerializeField] private Transform hudParent;

    private Dictionary<string, GameObject> robotHUDs = new();

    private void OnEnable()
    {
        swarmState.OnSwarmChanged += RefreshHUD;
    }

    private void OnDisable()
    {
        swarmState.OnSwarmChanged -= RefreshHUD;
    }

    private void Start()
    {
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        List<string> toRemove = new();
        foreach (var kvp in robotHUDs)
        {
            if (!swarmState.SwarmUnits.Any(u => u.id == kvp.Key && u.isAlive))
                toRemove.Add(kvp.Key);
        }
        foreach (var id in toRemove)
        {
            Destroy(robotHUDs[id]);
            robotHUDs.Remove(id);
        }

        foreach (var unitData in swarmState.GetAliveUnits())
        {
            if (!robotHUDs.ContainsKey(unitData.id))
            {
                GameObject hudGO = Instantiate(robotHUDPrefab, hudParent);
                robotHUDs[unitData.id] = hudGO;
                var hud = hudGO.GetComponent<RobotHUD>();
                hud.Setup(unitData);
            }
        }
    }
}