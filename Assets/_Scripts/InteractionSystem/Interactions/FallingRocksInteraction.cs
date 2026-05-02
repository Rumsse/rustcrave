using System.Collections.Generic;

using UnityEngine;



public class FallingRocksInteraction : InteractableBase

{

    [SerializeField] private AOEProjectile _projectile;

    [SerializeField] private Transform _hitPoint;

    [SerializeReference] private List<EffectBase> _effects;

    [SerializeField] private bool _isSingleUse;

    [SerializeField] private int _damage;



    private bool _used;



    public override void OnInteract()

    {

        if (_isSingleUse && _used)

            return;



        var proj = PoolManager.Instance.Get(_projectile);

        proj.transform.position = _hitPoint.position;

        proj.Init(new DamageInfo(_damage, AttackType.Environmental), _projectile, _effects);

        _used = true;

    }

}