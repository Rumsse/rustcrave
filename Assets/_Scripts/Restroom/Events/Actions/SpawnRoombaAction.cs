using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActivateObjectAction", menuName = "Restroom/Events/Actions/Activate Object")]
public class ActivateObjectAction : EventAction
{
    [SerializeField] private string targetTag;
    [SerializeField] private bool setActive = true;

    public override void Execute(SwarmUnitsData selectedUnit, SwarmState swarmState)
    {
        if (!ObjectRegistry.Instance.TryGetObjects(targetTag, out List<GameObject> targets))
        {
            Debug.LogWarning($"ActivateObjectAction: No objects found with tag '{targetTag}'");
            return;
        }

        foreach (var target in targets)
            target.SetActive(setActive);
    }
}