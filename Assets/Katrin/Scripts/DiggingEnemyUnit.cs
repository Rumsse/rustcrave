using UnityEngine;
using FMODUnity;

public class DiggingEnemyUnit : EnemyUnit
{
    #region Serialized Fields

    [Header("Digging Enemy Details")]
    [SerializeField] private ParticleSystem _diggingEffect;

    [Header("Audio")]
    [SerializeField] private EventReference _digSound;
    [SerializeField] private EventReference _emergeSound;
    [SerializeField] private EventReference _moleAttackSound;

    #endregion

    #region Private Fields

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    #endregion

    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        HandleSpecialEffects();
    }

    #endregion

    #region Special Behavior

    protected override void HandleSpecialReaction()
    {
        playerUnits.Clear();
        HandleMovement(guardPoint.position);
    }

    public override void PrepareUnit()
    {
        AudioManager.PlayOneShot(_emergeSound);
        selectedVisualObject.SetActive(true);
        _diggingEffect?.Stop();
        animator.Play(specialAnimationName);
    }

    public override void HandleSpecialEffects()
    {
        if (!selectedVisualObject.activeSelf) return;

        AudioManager.PlayOneShot(_digSound);
        animator.SetBool(IsWalkingHash, false);
        SetState(new IdleState(this));
        selectedVisualObject.SetActive(false);
        _diggingEffect?.Play();
    }

    #endregion

    #region Combat

    protected override void ExecuteAttackAction()
    {
        AudioManager.PlayOneShot(_moleAttackSound);
        currentAttack.Execute(AttackTarget, this);
    }

    #endregion

    #region Stolen Item

    public override void StealItem(ItemSO item)
    {
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