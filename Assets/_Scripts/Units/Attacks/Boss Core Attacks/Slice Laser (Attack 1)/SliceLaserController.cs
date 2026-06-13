using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using FMODUnity;

public class SliceLaserController : MonoBehaviour
{
    [SerializeField] private List<Laser> _lasers;

    [SerializeField] private EventReference _chargeSound;
    [SerializeField] private EventReference _fireSound;

    [SerializeField] private float _laserRange = 50f;
    [SerializeField] private float _shakeIntensity = 1f;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _sequenceBuffer = 0.1f;
    [SerializeField] private float _scaleMultiplier = 2f;

    private readonly List<MeshRenderer> _laserMeshes = new();
    private SliceLaserAttack _attackData;
    private DamageInfo _damageInfo;
    private UnitBase _attacker;
    private List<EffectBase> _effects;
    private int _sliceCount;

    private MaterialPropertyBlock _propertyBlock;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");

    private Sequence _initSequence;
    private Sequence _rotationSequence;
    private Tween _colorTween;

    #region Unity Lifecycle & Init

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        GetRenderers();
    }

    private void OnDisable()
    {
        if (_initSequence.isAlive)
            _initSequence.Stop();

        if (_rotationSequence.isAlive)
            _rotationSequence.Stop();

        if (_colorTween.isAlive)
            _colorTween.Stop();

        if (_attacker != null)
            _attacker.IsPerformingSpecial = false;
    }

    public void Init(DamageInfo damageInfo, SliceLaserAttack attackData, UnitBase attacker, List<EffectBase> effects)
    {
        _damageInfo = damageInfo;
        _attackData = attackData;
        _attacker = attacker;
        _effects = effects;
        _sliceCount = attackData.sliceAmount;

        SetColors(_attackData.startColor);
        ScaleLasersInstant(0f);
        ScaleIndicatorInstant(0f);

        PlaySequence();
    }

    #endregion

    #region Sequencing

    private void PlaySequence()
    {
        _initSequence = Sequence.Create()
            .ChainCallback(() => AudioManager.PlayOneShot(_chargeSound))
            .ChainCallback(() => ScaleIndicators(1f))
            .ChainDelay(_attackData.indicatorScaleSettings.duration + _sequenceBuffer)
            .ChainCallback(RotationRecursion);
    }

    private void RotationRecursion()
    {
        Sequence seq = Sequence.Create()
            .Chain(Tween.LocalRotation(transform, new TweenSettings<Quaternion>(transform.localRotation * GetRandomRotation(), _attackData.rotationSettings)))
            .ChainDelay(_attackData.delayAfterStopping)
            .ChainCallback(() => ChangeColors(true))
            .ChainDelay(_attackData.colorTweenSettings.duration)
            .ChainCallback(FireLasers)
            .ChainDelay(_attackData.recursionDelay + _attackData.laserScaleSettings.duration * _scaleMultiplier);

        if (_sliceCount > 1)
        {
            seq.ChainCallback(() => AudioManager.PlayOneShot(_chargeSound));
            seq.ChainCallback(RotationRecursion);
        }
        else
        {
            seq.ChainCallback(() => ScaleIndicators(0f))
                .ChainDelay(_attackData.indicatorScaleSettings.duration)
                .ChainCallback(() =>
                {
                    if (_attacker != null)
                        _attacker.IsPerformingSpecial = false;
                })
                .ChainCallback(() => PoolManager.Instance.Release(this, _attackData.controllerPrefab));
        }

        _sliceCount--;
        _rotationSequence = seq;
    }

    #endregion

    #region Firing Lasers

    private void FireLasers()
    {
        AudioManager.PlayOneShot(_fireSound);
        ChangeColors(false);
        Tween.ShakeCamera(Camera.main, _shakeIntensity, duration: _shakeDuration);
        ScaleLasers(1f);

        foreach (var laser in _lasers)
            FireLaserInDirection(laser);
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

            if (_effects.Count > 0 && result.attachedRigidbody.TryGetComponent(out IAffectable affectable))
                foreach (var effect in _effects)
                    affectable.ApplyEffect(effect);
        }
    }

    #endregion

    #region Helpers

    private void GetRenderers()
    {
        for (var index = 0; index < _lasers.Count; index++)
            _laserMeshes.Add(_lasers[index].Visuals.GetComponent<MeshRenderer>());
    }

    private Quaternion GetRandomRotation() => Quaternion.Euler(0f, Random.Range(_attackData.rotationMin, _attackData.rotationMax), 0f);

    #region Scale

    private void ScaleLasers(float value)
    {
        foreach (var laser in _lasers)
            laser.SetVisualsScale(value, _attackData.laserScaleSettings);
    }

    private void ScaleLasersInstant(float value)
    {
        foreach (var laser in _lasers)
            laser.SetVisualsScaleInstant(value);
    }

    private void ScaleIndicators(float value)
    {
        foreach (var laser in _lasers)
            laser.SetIndicatorWidth(value, _attackData.indicatorScaleSettings);
    }

    private void ScaleIndicatorInstant(float value)
    {
        foreach (var laser in _lasers)
            laser.SetIndicatorWidthInstant(value);
    }

    #endregion

    private void SetColors(Color color)
    {
        foreach (var mesh in _laserMeshes)
        {
            mesh.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(ColorProp, color);
            mesh.SetPropertyBlock(_propertyBlock);
        }
    }

    private void ChangeColors(bool toEndValue)
    {
        Color startColor = toEndValue ? _attackData.startColor : _attackData.endColor;
        Color endColor = toEndValue ? _attackData.endColor : _attackData.startColor;

        _colorTween = Tween.Custom(startColor, endColor, _attackData.colorTweenSettings, SetColors);
    }

    #endregion
}