using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class TargetLaserController : MonoBehaviour
{
    [SerializeField] private Laser _laser;

    private MeshRenderer _laserMesh;
    private TargetLaserAttack _attackData;
    private DamageInfo _damageInfo;
    private UnitBase _attacker;
    private List<EffectBase> _effects;
    private Unit _targetUnit;

    private MaterialPropertyBlock _propertyBlock;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");

    #region Unity Lifecycle & Init

    private void Awake()
    {
        _laserMesh = _laser.Visuals.GetComponent<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void Init(DamageInfo damageInfo, TargetLaserAttack attackData, UnitBase attacker, List<EffectBase> effects)
    {
        _damageInfo = damageInfo;
        _attackData = attackData;
        _attacker = attacker;
        _effects = effects;

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
            .ChainCallback(() => ScaleIndicators(1))
            .ChainDelay(_attackData.indicatorScaleSettings.duration + _attackData.timeToFire)
            .ChainCallback(FireLasers)
            .ChainDelay(_attackData.laserScaleSettings.duration * 2f + .1f)
            .ChainCallback(() => _attacker.IsPerformingSpecial = false)
            .ChainCallback(() => PoolManager.Instance.Release(this, _attackData.controllerPrefab));
    }

    #endregion

    #region Firing Lasers

    private void FireLasers()
    {
        ChangeColors(false);
        Tween.ShakeCamera(Camera.main, 1f, duration: .2f);
        ScaleLasers(1);

        FireLaserInDirection(_laser);
    }

    private void FireLaserInDirection(Laser laser)
    {
        var startPos = laser.transform.position;
        float range = 50f;

        Vector3 halfExtents = new Vector3(
            laser.InitialLaserScale.x / 2f,
            laser.InitialLaserScale.z / 2f,
            range
        );

        Vector3 center = startPos + laser.transform.forward * range;
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