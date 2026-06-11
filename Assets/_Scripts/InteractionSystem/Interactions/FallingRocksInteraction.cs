using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class FallingRocksInteraction : InteractableBase
{
    #region Inspector Fields

    [SerializeField] private AOEProjectile _projectile;
    [SerializeField] private Transform _hitPoint;
    [SerializeReference] private List<EffectBase> _effects;
    [SerializeField] private bool _isSingleUse;
    [SerializeField] private int _damage;
    [SerializeField] private UnityEvent _onTriggered;
    [SerializeField] private EventReference _breakSound;

    #endregion

    #region Private Fields

    private bool _used;

    #endregion

    #region Logic

    public override void OnInteract()
    {
        if (_isSingleUse && _used)
            return;

        SpawnProjectile();
        PlayBreakSound();

        if (_onTriggered != null)
            _onTriggered.Invoke();

        _used = true;
    }

    private void SpawnProjectile()
    {
        var proj = PoolManager.Instance.Get(_projectile);
        proj.transform.position = _hitPoint.position;
        proj.Init(new DamageInfo(_damage, AttackType.Environmental), _projectile, _effects);
    }

    private void PlayBreakSound()
    {
        AudioManager.PlayOneShot(_breakSound);
    }

    #endregion
}