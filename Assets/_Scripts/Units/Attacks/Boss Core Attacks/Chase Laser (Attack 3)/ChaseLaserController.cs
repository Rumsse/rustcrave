using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class ChaseLaserController : MonoBehaviour
{
    [SerializeField] private Laser _laser;
    [SerializeField] private DamageOnTriggerEnter _damageOnTriggerEnter;

    [SerializeField] private EventReference _chargeSound;
    [SerializeField] private EventReference _fireSound;

    [SerializeField] private float _sequenceBuffer = 0.1f;

    private MeshRenderer _laserMesh;
    private ChaseLaserAttack _attackData;
    private DamageInfo _damageInfo;
    private UnitBase _attacker;
    private List<EffectBase> _effects;

    private MaterialPropertyBlock _propertyBlock;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");

    private EventInstance _fireSoundInstance;
    private Sequence _sequence;
    private Tween _colorTween;

    #region Unity Lifecycle & Init

    private void Awake()
    {
        _laserMesh = GetComponentInChildren<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        StopLoopingSound();

        if (_sequence.isAlive)
            _sequence.Stop();

        if (_colorTween.isAlive)
            _colorTween.Stop();

        if (_attacker != null)
            _attacker.IsPerformingSpecial = false;
    }

    public void Init(DamageInfo damageInfo, ChaseLaserAttack attackData, UnitBase attacker, List<EffectBase> effects)
    {
        _damageInfo = damageInfo;
        _attackData = attackData;
        _attacker = attacker;
        _effects = effects;

        _damageOnTriggerEnter.Init(_damageInfo, _attacker.gameObject);

        SetColors(_attackData.startColor);
        ScaleLasersInstant(0f);
        ScaleIndicatorInstant(0f);

        PlaySequence();
    }

    #endregion

    #region Sequencing

    private void PlaySequence()
    {
        _sequence = Sequence.Create()
            .ChainCallback(() => AudioManager.PlayOneShot(_chargeSound))
            .ChainCallback(() => ScaleIndicators(1f))
            .ChainDelay(_attackData.indicatorScaleSettings.duration + _attackData.timeToFire)
            .ChainCallback(FireLaser)
            .ChainDelay(_attackData.laserScaleSettings.duration + _sequenceBuffer)
            .Chain(Tween.LocalRotation(
                transform,
                new TweenSettings<Quaternion>(
                    Quaternion.Euler(transform.rotation.x, transform.rotation.y + _attackData.rotateAmount, transform.rotation.z),
                    _attackData.rotationSettings)))
            .ChainDelay(_sequenceBuffer)
            .ChainCallback(StopLoopingSound)
            .ChainCallback(() => ScaleLasers(0f))
            .ChainCallback(() => ScaleIndicators(0f))
            .ChainDelay(_attackData.laserScaleSettings.duration + _sequenceBuffer)
            .ChainCallback(() =>
            {
                if (_attacker != null)
                    _attacker.IsPerformingSpecial = false;
            })
            .ChainCallback(() => PoolManager.Instance.Release(this, _attackData.controllerPrefab));
    }

    private void FireLaser()
    {
        _fireSoundInstance = AudioManager.CreateInstance(_fireSound);
        _fireSoundInstance.start();

        ChangeColors(false);
        ScaleLasers(1f);
    }

    #endregion

    #region Helpers

    private void StopLoopingSound()
    {
        _fireSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _fireSoundInstance.release();
    }

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

        _colorTween = Tween.Custom(this, new TweenSettings<Color>(startColor, endColor, _attackData.colorTweenSettings), (target, color) => target.SetColors(color));
    }

    #region Scale

    private void ScaleLasers(float value) => _laser.SetVisualsScale(value, _attackData.laserScaleSettings);

    private void ScaleLasersInstant(float value) => _laser.SetVisualsScaleInstant(value);

    private void ScaleIndicators(float value) => _laser.SetIndicatorWidth(value, _attackData.indicatorScaleSettings);

    private void ScaleIndicatorInstant(float value) => _laser.SetIndicatorWidthInstant(value);

    #endregion

    #endregion
}