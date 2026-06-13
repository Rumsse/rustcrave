using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using FMODUnity;

public class TargetLaserController : MonoBehaviour
{
    [SerializeField] private Laser _laser;

    [SerializeField] private EventReference _chargeSound;
    [SerializeField] private EventReference _fireSound;

    [SerializeField] private float _laserRange = 50f;
    [SerializeField] private float _shakeIntensity = 1f;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _sequenceBuffer = 0.1f;
    [SerializeField] private float _scaleMultiplier = 2f;

    private MeshRenderer _laserMesh;
    private TargetLaserAttack _attackData;
    private DamageInfo _damageInfo;
    private UnitBase _attacker;
    private List<EffectBase> _effects;
    private Unit _targetUnit;

    private MaterialPropertyBlock _propertyBlock;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private Action _onComplete;

    #region Unity Lifecycle & Init

    private void Awake()
    {
        _laserMesh = _laser.Visuals.GetComponent<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void Init(DamageInfo damageInfo, TargetLaserAttack attackData, UnitBase attacker, List<EffectBase> effects, Action onComplete = null)
    {
        _damageInfo = damageInfo;
        _attackData = attackData;
        _attacker = attacker;
        _effects = effects;
        _onComplete = onComplete;

        _targetUnit = Unit.GetRandomUnit();

        SetColors(_attackData.startColor);
        ScaleLasersInstant(0);
        ScaleIndicatorInstant(0);

        PlaySequence();
    }

    #endregion

    #region Sequencing

    private void PlaySequence()
    {
        transform.localRotation = Quaternion.LookRotation(_targetUnit.transform.position - _attacker.transform.position);

        Sequence.Create()
            .ChainCallback(() => AudioManager.PlayOneShot(_chargeSound))
            .ChainCallback(() => ScaleIndicators(1))
            .ChainDelay(_attackData.indicatorScaleSettings.duration + _attackData.timeToFire)
            .ChainCallback(FireLasers)
            .ChainDelay(_attackData.laserScaleSettings.duration * _scaleMultiplier + _sequenceBuffer)
            .ChainCallback(() => _onComplete?.Invoke())
            .ChainCallback(() => PoolManager.Instance.Release(this, _attackData.controllerPrefab));
    }

    #endregion

    #region Firing Lasers

    private void FireLasers()
    {
        AudioManager.PlayOneShot(_fireSound);
        ChangeColors(false);
        Tween.ShakeCamera(Camera.main, _shakeIntensity, duration: _shakeDuration);
        ScaleLasers(1);

        FireLaserInDirection(_laser);
    }

    private void FireLaserInDirection(Laser laser)
    {
        var startPos = laser.transform.position;

        Vector3 halfExtents = new Vector3(
            laser.InitialLaserScale.x / _scaleMultiplier,
            laser.InitialLaserScale.z / _scaleMultiplier,
            _laserRange
        );

        Vector3 center = startPos + laser.transform.forward * _laserRange;
        Quaternion orientation = laser.transform.rotation;

        var overlapResult = Physics.OverlapBox(center, halfExtents, orientation, _attackData.mask);

        if (overlapResult.Length == 0)
            return;

        foreach (var result in overlapResult)
        {
            if (!result.attachedRigidbody)
                continue;

            if (result.attachedRigidbody.gameObject == _attacker.gameObject)
                continue;

            if (result.attachedRigidbody.TryGetComponent(out IDamageable damageable))
                damageable.Hit(_damageInfo);

            if (_effects.Count == 0 || !result.attachedRigidbody.TryGetComponent(out IAffectable affectable))
                continue;

            foreach (var effect in _effects)
                affectable.ApplyEffect(effect);
        }
    }

    #endregion

    #region Helpers

    private void SetColors(Color color)
    {
        _laserMesh.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(ColorProp, color);
        _laserMesh.SetPropertyBlock(_propertyBlock);
    }

    private void ChangeColors(bool toEndValue)
    {
        Color startColor = toEndValue ? _attackData.startColor : _attackData.endColor;
        Color endColor = toEndValue ? _attackData.endColor : _attackData.startColor;

        Tween.Custom(startColor, endColor, _attackData.colorTweenSettings, SetColors);
    }

    #region Scale

    private void ScaleLasers(float value) => _laser.SetVisualsScale(value, _attackData.laserScaleSettings);

    private void ScaleLasersInstant(float value) => _laser.SetVisualsScaleInstant(value);

    private void ScaleIndicators(float value) => _laser.SetIndicatorWidth(value, _attackData.indicatorScaleSettings);

    private void ScaleIndicatorInstant(float value) => _laser.SetIndicatorWidthInstant(value);

    #endregion

    #endregion
}