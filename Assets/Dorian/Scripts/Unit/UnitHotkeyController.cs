using System.Collections.Generic;
using UnityEngine;

public class UnitHotkeyController : MonoBehaviour
{
    [SerializeField] private SwarmState swarmState;

    private Dictionary<KeyCode, string> hotkeys = new();

    private void OnEnable()
    {
        swarmState.OnSwarmChanged += RebuildHotkeys;
    }

    private void OnDisable()
    {
        swarmState.OnSwarmChanged -= RebuildHotkeys;
    }

    private void Start()
    {
        RebuildHotkeys();
    }

    private void Update()
    {
        foreach (var pair in hotkeys)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                SelectUnit(pair.Value);
            }
        }
    }

    private void RebuildHotkeys()
    {
        hotkeys.Clear();

        var alive = swarmState.GetAliveUnits();

        for (int i = 0; i < alive.Count; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;

            if (key > KeyCode.Alpha9)
                break;

            hotkeys[key] = alive[i].id;
        }
    }

    private void SelectUnit(string id)
    {
        Unit unit = UnitRegistry.Get(id);

        if (unit != null)
        {
            UnitSelectionSystem.Instance.SetSelectedUnit(unit);
        }
    }
}