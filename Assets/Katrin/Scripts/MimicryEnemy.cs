using PrimeTween;
using UnityEngine;

public class MimicryEnemy : EnemyUnit
{
    [SerializeField] private GameObject mimicryObject;
    [SerializeField] private GameObject legsModel;
    [SerializeField] private float offset = 1.5f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float magnitude = 0.001f;
    [SerializeField] private float hiddenYOffset = -1.5f;
    [SerializeField] private float emergeDuration = 0.5f;

    private Vector3 _startLocalPos;
    private bool _isMimicking = true;
    private Sequence _shakeSequence;

    protected override void Start()
    {
        base.Start();

        _isMimicking = true;
        _startLocalPos = mimicryObject.transform.localPosition;

        mimicryObject.transform.localPosition = _startLocalPos + new Vector3(0, hiddenYOffset, 0);

        if (legsModel)
            legsModel.SetActive(false);

        if (agent)
            agent.enabled = false;

        StartShakeSequence();
    }

    public override void PrepareUnit()
    {
        if (!_isMimicking)
            return;

        _isMimicking = false;
        _shakeSequence.Stop();

        if (legsModel)
            legsModel.SetActive(true);

        selectedVisualObject.SetActive(true);
        animator.Play(specialAnimationName);

        Tween.LocalPosition(mimicryObject.transform, _startLocalPos, emergeDuration, Ease.OutBack)
            .OnComplete(OnEmerged);
    }

    public override bool FarDetectEnabled() => !_isMimicking;

    #region Tweening

    private void StartShakeSequence()
    {
        _shakeSequence = Sequence.Create(cycles: -1)
            .Chain(Tween.Delay(offset))
            .Chain(Tween.ShakeLocalPosition(mimicryObject.transform, strength: new Vector3(magnitude, 0, magnitude), duration: duration));
    }

    private void OnEmerged()
    {
        if (agent)
            agent.enabled = true;
    }

    #endregion
}