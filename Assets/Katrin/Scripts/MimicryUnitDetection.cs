using UnityEngine;

public class MimicryUnitDetection : PlayerUnitDetector
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (!unit) return;

        if (!other.CompareTag("Unit"))
            return;

        if (!other.TryGetComponent(out Unit unitObj))
            return;

        // add here logic with extra damage handling
        unit.UnitEnter(unitObj);
    }

    protected override void OnTriggerExit(Collider other)
    {
        // add here logic with extra damage handling
    }
}
