using UnityEngine;

public class DiggingEnemyUnit : EnemyUnit
{
    [Header("Digging Enemy Details")]
    [SerializeField] private string specialAnimationName;

    protected override void Start()
    {
        base.Start();
        selectedVisualObject.SetActive(false);
    }
    protected override void HandleSpecialReaction()
    {
        playerUnits.Clear();
        HandleMovement(guardPoint.position);
    }

    public override void PrepareUnit()
    {
        selectedVisualObject.SetActive(true);
        animator.Play(specialAnimationName);
    }

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
        selectedVisualObject.SetActive(false);
        base.ClearStolenItem();
    }

    #endregion

}
