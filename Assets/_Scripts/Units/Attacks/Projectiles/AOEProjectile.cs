using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
    [Tooltip("Parent that contains empties with colliders. By default it should be disabled.")]
    [SerializeField] private GameObject _hitboxesParent;
    
    [SerializeField] private ParticleSystem _effect;
    [SerializeField] private Animation _animation;
    
    private DamageInfo _damage;

    private AOEProjectile _prefabOrigin;
    private List<EffectBase> _effects = new();
    
    public void Init(DamageInfo damage, AOEProjectile prefabOrigin, List<EffectBase> effects)
    {
        _damage = damage;
        _prefabOrigin = prefabOrigin;
        _effects = effects;
        
        StartCoroutine(HitC());
    }

    private IEnumerator HitC()
    {
        _hitboxesParent.SetActive(true);
        if(_effect) _effect.Play();
        if(_animation) _animation.Play();
        
        yield return new WaitForFixedUpdate();
        
        _hitboxesParent.SetActive(false);

        yield return new WaitWhile(AreVisualsActive);
        
        PoolManager.Instance.Release(this, _prefabOrigin);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Unit"))
            return;
        
        if(!other.TryGetComponent(out IDamageable damageable))
            return;

        if(_effects.Count > 0 && other.TryGetComponent(out IAffectable affectable))
            foreach (var effect in _effects)
                affectable.ApplyEffect(effect);
        
        damageable.Hit(_damage);
    }

    private bool AreVisualsActive()
    {
        if (_effect && _effect.isPlaying) return true;

        if (_animation && _animation.isPlaying) return true;
        
        return false;
    }
}
