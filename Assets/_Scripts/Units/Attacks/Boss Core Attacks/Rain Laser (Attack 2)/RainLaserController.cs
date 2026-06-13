using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using Utils;
using FMODUnity;

public class RainLaserController : MonoBehaviour
{
    [SerializeField] private EventReference _chargeSound;
    [SerializeField] private EventReference _fireSound;

    [SerializeField] private float _shakeIntensity = 1f;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _sphereRadiusDivider = 2f;
    [SerializeField] private float _sequenceBuffer = 0.1f;
    [SerializeField] private float _scaleMultiplier = 2f;

    private RainLaserAttack _attackData;
    private DamageInfo _damageInfo;
    private UnitBase _attacker;
    private List<EffectBase> _effects;

    private readonly List<Vector3> _hitPoints = new();
    private MaterialPropertyBlock _propertyBlock;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");

    private Sequence _sequence;
    private Tween _colorTween;

    #region Init

    private void Awake() => _propertyBlock = new MaterialPropertyBlock();

    private void OnDisable()
    {
        if (_sequence.isAlive)
            _sequence.Stop();

        if (_colorTween.isAlive)
            _colorTween.Stop();

        StopAllCoroutines();

        if (_attacker != null)
            _attacker.IsPerformingSpecial = false;
    }

    public void Init(DamageInfo damageInfo, RainLaserAttack attackData, UnitBase attacker, List<EffectBase> effects)
    {
        _damageInfo = damageInfo;
        _attackData = attackData;
        _attacker = attacker;
        _effects = effects;

        _hitPoints.Clear();
        GetHitPoints();

        PlaySequence();
    }

    #endregion

    #region Sequencing

    private void PlaySequence() => FireLaserRecursion(0);

    private void FireLaserRecursion(int index)
    {
        var laser = PoolManager.Instance.Get(_attackData.laserPrefab);
        laser.transform.position = _hitPoints[index];

        ResetLaser(laser);

        Sequence seq = Sequence.Create()
            .ChainCallback(() => AudioManager.PlayOneShot(_chargeSound))
            .ChainCallback(() => ScaleIndividualIndicator(1, laser))
            .ChainDelay(_attackData.indicatorScaleSettings.duration + _attackData.timeToFire)
            .ChainCallback(() => FireLaser(laser))
            .ChainDelay(_attackData.laserScaleSettings.duration * _scaleMultiplier + _attackData.attackDelay)
            .ChainCallback(() => PoolManager.Instance.Release(laser, _attackData.laserPrefab));

        if (index < _attackData.hitAmount - 1)
        {
            seq.ChainCallback(() => FireLaserRecursion(index + 1));
        }
        else
        {
            seq.ChainCallback(() =>
            {
                if (_attacker != null)
                    _attacker.IsPerformingSpecial = false;
            })
               .ChainCallback(() => PoolManager.Instance.Release(this, _attackData.controllerPrefab));
        }

        _sequence = seq;
    }

    #endregion

    #region Firing Laser

    private void FireLaser(Laser laser)
    {
        AudioManager.PlayOneShot(_fireSound);
        ChangeColor(laser, false);
        Tween.ShakeCamera(Camera.main, _shakeIntensity, duration: _shakeDuration);
        ScaleIndividualLasers(1, laser);
        ScaleIndividualIndicator(0, laser);

        StartCoroutine(TryHit(laser));
    }

    private IEnumerator TryHit(Laser laser)
    {
        yield return new WaitForSeconds(_attackData.laserScaleSettings.duration);

        var overlapResult = Physics.OverlapSphere(
            laser.transform.position,
            laser.Visuals.localScale.x / _sphereRadiusDivider,
            _attackData.mask);

        if (overlapResult.Length == 0)
            yield break;

        foreach (var result in overlapResult)
        {
            if (!result.attachedRigidbody)
                continue;

            if (result.attachedRigidbody.gameObject == _attacker.gameObject)
                continue;

            if (result.attachedRigidbody.TryGetComponent(out IDamageable damageable))
                damageable.Hit(_damageInfo);

            if (_effects.Count > 0 && result.attachedRigidbody.TryGetComponent(out IAffectable affectable))
                foreach (var effect in _effects)
                    affectable.ApplyEffect(effect);
        }
    }

    #endregion

    #region Helpers

    private void SetColor(Laser laser, Color color)
    {
        var rend = laser.Visuals.GetComponent<MeshRenderer>();
        rend.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(ColorProp, color);
        rend.SetPropertyBlock(_propertyBlock);
    }

    private void ChangeColor(Laser laser, bool toEndValue)
    {
        Color startColor = toEndValue ? _attackData.startColor : _attackData.endColor;
        Color endColor = toEndValue ? _attackData.endColor : _attackData.startColor;

        _colorTween = Tween.Custom(startColor, endColor, _attackData.colorTweenSettings, color => SetColor(laser, color));
    }

    #region Scale

    private void ScaleIndividualLasersInstant(float value, Laser laser) => laser.SetVisualsScaleInstant(value);

    private void ScaleIndividualLasers(float value, Laser laser) => laser.SetVisualsScale(value, _attackData.laserScaleSettings);

    private void ScaleIndividualIndicatorInstant(float value, Laser laser)
    {
        laser.SetIndicatorWidthInstant(value);
        laser.SetIndicatorHeightInstant(value);
    }

    private void ScaleIndividualIndicator(float value, Laser laser)
    {
        laser.SetIndicatorWidth(value, _attackData.indicatorScaleSettings);
        laser.SetIndicatorHeight(value, _attackData.indicatorScaleSettings);
    }

    #endregion

    private void GetHitPoints()
    {
        for (int i = 0; i < _attackData.hitAmount; i++)
        {
            Vector3 hitPoint = Vector3.zero;
            if (i < _attackData.attacksAtUnit)
                hitPoint = NavMeshUtils.GetNearestNavMeshPoint(Unit.GetRandomUnit().transform.position);
            else
                hitPoint = NavMeshUtils.GetRandomNavMeshPoint(_attacker.transform.position, _attackData.possibleHitRange);

            _hitPoints.Add(hitPoint);
        }

        _hitPoints.Shuffle();
    }

    private void ResetLaser(Laser laser)
    {
        SetColor(laser, _attackData.startColor);
        ScaleIndividualIndicatorInstant(0, laser);
        ScaleIndividualLasersInstant(0, laser);
    }

    #endregion
}