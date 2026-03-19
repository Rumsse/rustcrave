using UnityEngine;

public class DamageOnTriggerEnter : MonoBehaviour
{
    [SerializeField] private bool _hitEnemyUnits;
    [SerializeField] private bool _hitPlayerUnits;
    [SerializeField] private bool _hitSelf;
    [SerializeField] private LayerMask _layerMask;

    private GameObject _attacker;
    private DamageInfo _damageInfo;

    public void Init(DamageInfo damageInfo, GameObject attacker)
    {
        _attacker = attacker;
        _damageInfo = damageInfo;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if((1 << other.gameObject.layer) != _layerMask) 
            return;
        
        if (!_hitSelf && other.attachedRigidbody.gameObject == _attacker) return;
        if (!other.attachedRigidbody.TryGetComponent(out UnitBase unit))
        {
            if (!_hitEnemyUnits && unit is EnemyUnit) return;
            if (!_hitPlayerUnits && unit is Unit) return;
        }
        
        if (!other.attachedRigidbody.TryGetComponent(out IDamageable damageable))
            return;
        
        damageable.Hit(_damageInfo);
    }
}
