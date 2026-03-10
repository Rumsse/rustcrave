using UnityEngine;

public class Projectile : MonoBehaviour
{
    private DamageInfo _damage;
    private UnitBase _target;
    private float _projectileSpeed;

    private Transform _targetTransform;

    private Projectile _prefabOrigin;
    
    public void Init(DamageInfo damage, float projectileSpeed, UnitBase target, Projectile prefabOrigin)
    {
        _damage = damage;
        _target = target;
        _projectileSpeed = projectileSpeed;
        _targetTransform = target.ModelMidPoint;
        _prefabOrigin = prefabOrigin;
    }

    private void FixedUpdate()
    {
        if (!_target)
            return;
        
        transform.position = Vector3.MoveTowards(transform.position, _targetTransform.position, _projectileSpeed * Time.fixedDeltaTime);

        Vector3 dist = _targetTransform.position - transform.position;
        if (dist.sqrMagnitude < 0.1f)
        {
            _target?.HealthManager?.Hit(_damage);   
            PoolManager.Instance.Release(this, _prefabOrigin);
        }
    }
}
