using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class ChaseLaserController : MonoBehaviour
{
    [SerializeField] private Laser _laser;
    [SerializeField] private DamageOnTriggerEnter _damageOnTriggerEnter;
    private MeshRenderer _laserMesh;

    private ChaseLaserAttack _attackData;
    private DamageInfo _damageInfo;
    private UnitBase _attacker;
    private List<EffectBase> _effects;

    #region Unity Lifecycle & Init

    private void Awake()
    {
        _laserMesh = GetComponentInChildren<MeshRenderer>();
    }
    
    public void Init(DamageInfo damageInfo, ChaseLaserAttack attackData, UnitBase attacker, List<EffectBase> effects)
    {
        _damageInfo = damageInfo;
        _attackData = attackData;
        _attacker = attacker;
        _effects = effects;
        
        _damageOnTriggerEnter.Init(_damageInfo, _attacker.gameObject);
        
        SetColors();
        ScaleLasersInstant(0);
        ScaleIndicatorInstant(0);
        
        PlaySequence();
    }
    
    #endregion

    #region Sequencing

    private void PlaySequence()
    {
        Sequence.Create()
            .ChainCallback(() => ScaleIndicators(1f))
            .ChainDelay(_attackData.indicatorScaleSettings.duration + _attackData.timeToFire)
            .ChainCallback(() => ScaleLasers(1f))
            .ChainDelay(_attackData.laserScaleSettings.duration + .1f)
            .Chain(Tween.LocalRotation(
                transform,
                new TweenSettings<Quaternion>(
                    Quaternion.Euler(transform.rotation.x, transform.rotation.y + _attackData.rotateAmount, transform.rotation.z),
                    _attackData.rotationSettings)))
            .ChainDelay(.1f)
            .ChainCallback(() => ScaleLasers(0))
            .ChainCallback(() => ScaleIndicators(0))
            .ChainDelay(_attackData.laserScaleSettings.duration + .1f)
            .ChainCallback(() => _attacker.IsPerformingSpecial = false)
            .ChainCallback(() => PoolManager.Instance.Release(this, _attackData.controllerPrefab));
    }

    #endregion
    
    #region Helpers

    private void SetColors()
    {
        _laserMesh.material.color = _attackData.laserColor;
    }

    #region Scale

    private void ScaleLasers(float value)
    {
        _laser.SetVisualsScale(value, _attackData.laserScaleSettings);
    }

    private void ScaleLasersInstant(float value)
    {
        _laser.SetVisualsScaleInstant(value);
    }
    
    private void ScaleIndicators(float value)
    {
        _laser.SetIndicatorWidth(value, _attackData.indicatorScaleSettings);
    }

    private void ScaleIndicatorInstant(float value)
    {
        _laser.SetIndicatorWidthInstant(value);
    }

    #endregion

    #endregion
}
