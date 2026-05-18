using UnityEngine;
using UnityEngine.UIElements;

public class DiggingEnemyUnit : EnemyUnit
{
    [Header("Digging Details")]
    [SerializeField] private float diggingDuration;
    [SerializeField] private float multiplier;

    protected override void HandleSpecialReaction()
    {
        playerUnits.Clear();
        HandleMovement(guardPoint.position);
    }

    public (float, float) GetDiggingDetails() => (diggingDuration, multiplier);

    #region Stolen Item

    public override void StealItem(ItemSO item)
    {
        Debug.Log("Stealing item...");
        _available = false;
        base.StealItem(item);
        HandleSpecialReaction();
    }

    public override void ClearStolenItem()
    {
        _available = true;
        base.ClearStolenItem();
    }

    #endregion

}
