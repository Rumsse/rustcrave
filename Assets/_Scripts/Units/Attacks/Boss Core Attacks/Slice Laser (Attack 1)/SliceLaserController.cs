using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SliceLaserController : MonoBehaviour
{
    [SerializeField] private List<Transform> _lasers;
    private List<MeshRenderer> _laserMeshes = new();

    private SliceLaserAttack _attackData;
    private DamageInfo _damageInfo;
    private UnitBase _attacker;
    private List<EffectBase> _effects;
    private int _sliceCount;

    private void Awake()
    {
        GetRenderers();
    }

    private void GetRenderers()
    {
        for (var index = 0; index < _lasers.Count; index++)
        {
            var laser = _lasers[index];
            _laserMeshes.Add(laser.GetComponentInChildren<MeshRenderer>());
        }
    }

    private void SetColors()
    {
        foreach (var laser in _laserMeshes)
            laser.material.color = _attackData.colorSettings.startValue;
    }
    
    public void Init(DamageInfo damageInfo, SliceLaserAttack attackData, UnitBase attacker, List<EffectBase> effects)
    {
        _damageInfo = damageInfo;
        _attackData = attackData;
        _attacker = attacker;
        _effects = effects;
        _sliceCount = attackData.sliceAmount;
        
        SetColors();
        
        PlaySequence();
    }

    private void PlaySequence()
    {
        foreach (var laser in _lasers)
        {
            laser.localScale = Vector3.zero;
        }

        Sequence.Create()
            .ChainCallback(() => ScaleLasers(1f))
            .ChainDelay(_attackData.scaleSettings.duration + .1f)
            .ChainCallback(RotationRecursion);
    }

    private Quaternion GetRandomRotation()
    {
        Quaternion rotation = Quaternion.Euler(
            0,
            Random.Range(_attackData.rotationMin, _attackData.rotationMax),
            0);
        
        return rotation;
    }

    private void ScaleLasers(float value)
    {
        foreach (var laser in _lasers)
            Tween.Scale(laser, new TweenSettings<float>(value, _attackData.scaleSettings));
    }

    private void ChangeColors(bool endValue)
    {
        foreach (var laser in _laserMeshes)
            Tween.Custom(_attackData.colorSettings, color => laser.material.color = color);
    }
    
    private void RotationRecursion()
    {
        Sequence seq = Sequence.Create()
            .Chain(Tween.LocalRotation(transform, new TweenSettings<Quaternion>(transform.localRotation * GetRandomRotation(), _attackData.rotationSettings)))
            .ChainDelay(_attackData.delayAfterStopping)
            .ChainCallback(() => ChangeColors(true))
            .ChainDelay(_attackData.colorSettings.settings.duration)
            .ChainCallback(FireLasers)
            .ChainCallback(() => ChangeColors(false))
            .Group(Tween.ShakeCamera(Camera.main, 1f, duration: .2f))
            .ChainDelay(_attackData.recursionDelay);
            
        if(_sliceCount > 1) seq.ChainCallback(RotationRecursion);
        else
        {
            seq.ChainCallback(() => ScaleLasers(0))
                .ChainCallback(() => _attacker.IsPerformingSpecial = false);
        }
        
        _sliceCount--;
    }

    private void FireLasers()
    {
        foreach (var laser in _lasers)
        {
            FireLaserInDirection(laser);
        }
    }

    private void FireLaserInDirection(Transform laser)
    {
        var overlapResult = Physics.OverlapCapsule(
            laser.position,
            laser.position + laser.forward * 50, // 50 so it works like infinite range laser                        
            laser.localScale.magnitude / 2f,
            _attackData.mask);

        if (overlapResult.Length == 0) return;

        foreach (var result in overlapResult)
        {
            if (result.gameObject == _attacker.gameObject) continue;
            
            if(result.TryGetComponent(out IDamageable damageable))
                damageable.Hit(_damageInfo);
            
            if(_effects.Count > 0 && result.TryGetComponent(out IAffectable affectable))
                foreach (var effect in _effects)
                    affectable.ApplyEffect(effect);
        }
    }
}
