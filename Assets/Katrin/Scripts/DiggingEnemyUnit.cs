using UnityEngine;

public class DiggingEnemyUnit : EnemyUnit
{
    [Header("Digging Enemy Details")]
    [SerializeField] private ParticleSystem diggingEffect;

    protected override void Start()
    {
        base.Start();
        HandleSpecialEffects();
    }
    protected override void HandleSpecialReaction()
    {
        playerUnits.Clear();
        HandleMovement(guardPoint.position);
    }

    public override void PrepareUnit()
    {
        selectedVisualObject.SetActive(true);
        diggingEffect?.Stop();
        animator.Play(specialAnimationName);
    }

    public override void HandleSpecialEffects()
    {
        if (!selectedVisualObject.activeSelf) return;

        animator.SetBool("IsWalking", false); 
        SetState(new IdleState(this)); 
        selectedVisualObject.SetActive(false);
        diggingEffect?.Play();
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
        base.ClearStolenItem();
    }

    #endregion

}
