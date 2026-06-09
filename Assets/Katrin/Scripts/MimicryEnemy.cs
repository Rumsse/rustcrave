using UnityEngine;

public class MimicryEnemy : EnemyUnit
{
    public override void PrepareUnit()
    {
        selectedVisualObject.SetActive(true);
        animator.Play(specialAnimationName);
    }

    public override void HandleSpecialEffects()
    {
        if (!selectedVisualObject.activeSelf) return;

        animator.SetBool("IsWalking", false);
        SetState(new IdleState(this));
        selectedVisualObject.SetActive(false);
    }
}
